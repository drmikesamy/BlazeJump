using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using AutoMapper;
using BlazeJump.Common.Services.Crypto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlazeJump.Common.Services.Connections.Events;
using BlazeJump.Common.Services.UserProfile;
using System.Linq;

namespace BlazeJump.Common.Services.Message
{
	public class MessageService : IMessageService
	{
		private IRelayManager _relayManager;
		private ICryptoService _cryptoService;
		private IUserProfileService _userProfileService;
		private IMapper _mapper;
		private Dictionary<string, NEvent> sendMessageQueue = new Dictionary<string, NEvent>();
		public Dictionary<string, List<NMessage>> NMessages { get; set; } = new Dictionary<string, List<NMessage>>();
		public Dictionary<string, User> Users { get; set; } = new Dictionary<string, User>();
		private List<string> _usersToLoad { get; set; } = new List<string>();
		private List<string> _repliesToLoad { get; set; } = new List<string>();
		public event EventHandler StateUpdated;

		public MessageService(IRelayManager relayManager, ICryptoService cryptoService, IUserProfileService userProfileService, IMapper mapper)
		{
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_userProfileService = userProfileService;
			_mapper = mapper;
			_relayManager.NewMessageReceived += NewMessageReceived;
		}
		public async Task FetchNEventsByFilter(MessageTypeEnum requestMessageType, List<Filter> filters, string subscriptionId)
		{
			await _relayManager.QueryRelays(new List<string> { "wss://nostr.wine" }, subscriptionId, requestMessageType, filters);
		}
		private void LoadUsers()
		{
			if (_usersToLoad.Any())
			{
				var userFilter = new Filter
				{
					Kinds = new int[] { (int)KindEnum.Metadata },
					Since = DateTime.Now.AddYears(-20),
					Until = DateTime.Now.AddDays(1),
					Authors = _usersToLoad.ToList()
				};
				_ = FetchNEventsByFilter(MessageTypeEnum.Req, new List<Filter> { userFilter }, $"{TemplateAreaEnum.User}_{Guid.NewGuid()}");
				_usersToLoad.Clear();
			}
		}
		private void LoadReplies(MessageReceivedEventArgs e)
		{
			var filters = new List<Filter>();
			foreach (var parentEventId in _repliesToLoad)
			{
				var foundParentMessageGroup = NMessages.TryGetValue(e.Message.SubscriptionId, out var parentMessageGroup);
				if (foundParentMessageGroup)
				{
					filters.Add(new Filter
					{
						Kinds = new int[] { (int)KindEnum.Text },
						Since = parentMessageGroup.Single(m => m.Event.Id == parentEventId).Event.CreatedAtDateTime,
						Until = DateTime.Now.AddDays(1),
						TaggedEventIds = new List<string> { parentEventId }
					});
				}
			}
			if(filters.Count > 0)
			{
				_ = FetchNEventsByFilter(MessageTypeEnum.Req, filters, $"{TemplateAreaEnum.Replies}_{e.Message.SubscriptionId}");
			}
			_repliesToLoad.Clear();
		}
		private void NewMessageReceived(object sender, MessageReceivedEventArgs e)
		{
			if (e.Message.MessageType == MessageTypeEnum.Eose
				&& !e.Message.SubscriptionId.StartsWith($"{TemplateAreaEnum.Replies}_"))
			{
				LoadUsers();
				LoadReplies(e);
				return;
			}else if (e.Message.SubscriptionId.StartsWith($"{TemplateAreaEnum.User}_")
				&& !Users.ContainsKey(e.Message.Event.Pubkey))
			{
				ProcessUser(e.Message);
			}
			else if (e.Message.MessageType == MessageTypeEnum.Event)
			{
				NMessages.TryGetValue(e.Message.SubscriptionId, out var messages);
				if (messages != null)
				{
					messages.Add(e.Message);
				}
				else
				{
					NMessages.Add(e.Message.SubscriptionId, new List<NMessage> { e.Message });
				}
				if (!_usersToLoad.Any(u => u == e.Message.Event.Pubkey))
					_usersToLoad.Add(e.Message.Event.Pubkey);
				if (!_repliesToLoad.Any(r => r == e.Message.Event.Id))
					_repliesToLoad.Add(e.Message.Event.Id);
			}
			StateUpdated.Invoke(this, EventArgs.Empty);
		}
		private void ProcessUser(NMessage message)
		{
			try
			{
				var parsed = JObject.Parse(message.Event.Content);
				var user = new User
				{
					Id = message.Event.Pubkey,
					Username = parsed["name"]?.ToString(),
					Bio = parsed["about"]?.ToString(),
					ProfilePic = parsed["picture"]?.ToString(),
					Banner = parsed["banner"]?.ToString(),
				};
				if (Users.ContainsKey(user.Id))
				{
					Users[user.Id] = user;
				}
				else
				{
					Users.Add(user.Id, user);
				}

			}
			catch
			{

			}
		}
		private bool SignNEvent(ref NEvent nEvent)
		{
			if (_cryptoService.EtherealPublicKey == null)
			{
				_cryptoService.CreateEtherealKeyPair();
			}
			var signableEvent = nEvent.GetSignableNEvent();
			var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
			var signature = _cryptoService.Sign(serialisedNEvent);
			nEvent.Sig = signature;
			return true;
		}
		public bool VerifyNEvent(NEvent nEvent)
		{
			var signableEvent = nEvent.GetSignableNEvent();
			var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
			var verified = _cryptoService.Verify(nEvent.Sig, serialisedNEvent, nEvent.Pubkey);
			return verified;
		}
		public async Task SendNEvent(KindEnum kind, string message)
		{
			var nEvent = await GetNewNEvent(kind, message);
			var subscriptionHash = Guid.NewGuid().ToString();
			SignNEvent(ref nEvent);
			await _relayManager.SendNEvent(nEvent, new List<string> { "wss://nostr.wine" }, subscriptionHash);
			sendMessageQueue.TryAdd(nEvent.Id, nEvent);
		}
		private async Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId = null)
		{
			return new NEvent
			{
				Id = "0",
				Pubkey = _userProfileService.NPubKey,
				Kind = kind,
				Content = message,
				ParentNEventId = null,
				Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			};
		}
	}
}
