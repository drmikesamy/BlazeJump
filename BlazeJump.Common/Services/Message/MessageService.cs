using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using AutoMapper;
using BlazeJump.Common.Services.Crypto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlazeJump.Common.Services.Connections.Events;
using BlazeJump.Common.Services.UserProfile;
using Microsoft.Extensions.Logging;

namespace BlazeJump.Common.Services.Message
{
	public class MessageService : IMessageService
	{
		private IRelayManager _relayManager;
		private ICryptoService _cryptoService;
		private IUserProfileService _userProfileService;
		private IMapper _mapper;
		private Dictionary<string, NEvent> sendMessageQueue = new Dictionary<string, NEvent>();
		public List<NMessage> ReceivedMessageQueue { get; set; } = new List<NMessage>();
		public List<NEvent> NEvents => NMessages.Select(m => m.Event).ToList();
		public List<NMessage> NMessages { get; set; } = new List<NMessage>();
		public List<User> Users { get; set; } = new List<User>();
		public event EventHandler StateUpdated;

		public MessageService(IRelayManager relayManager, ICryptoService cryptoService, IUserProfileService userProfileService, IMapper mapper)
		{
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_userProfileService = userProfileService;
			_mapper = mapper;
			_relayManager.NewMessageReceived += NewMessageReceived;
		}
		public async Task FetchUserPage(string pubKey)
		{
			NMessages.Clear();
			var filter = new Filter
			{
				Kinds = new int[] { (int)KindEnum.Text },
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now.AddDays(1),
				Authors = new List<string> { pubKey },
				Limit = 5
			};
			await FetchNEventsByFilter(MessageTypeEnum.Req, filter);
		}
		public async Task FetchEventPage(string eventId)
		{
			NMessages.Clear();
			var filter = new Filter
			{
				Kinds = new int[] { (int)KindEnum.Text },
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now.AddDays(1),
				Limit = 1,
				Ids = new List<string> { eventId }
			};
			await FetchNEventsByFilter(MessageTypeEnum.Req, filter);
		}
		public async Task FetchReplies(List<string> eventIds, DateTime cutOffDate)
		{
			var filter = new Filter
			{
				Kinds = new int[] { (int)KindEnum.Text },
				Since = DateTime.Now.AddYears(-20),
				Until = cutOffDate,
				EventId = eventIds,
				Limit = 50
			};
			await FetchNEventsByFilter(MessageTypeEnum.Req, filter, $"reply_{Guid.NewGuid()}");
		}
		public async Task FetchNEventsByFilter(MessageTypeEnum requestMessageType, Filter filter, string subscriptionId = null)
		{
			var subscriptionHash = subscriptionId ?? Guid.NewGuid().ToString();
			await _relayManager.QueryRelays(new List<string> { "wss://nostr.wine" }, subscriptionHash, requestMessageType, filter);
		}
		private void NewMessageReceived(object sender, MessageReceivedEventArgs e)
		{
			if ((e.Message.SubscriptionId.StartsWith("user_") || e.Message.SubscriptionId.StartsWith("reply_")) && e.Message.MessageType != MessageTypeEnum.Eose)
			{
				ProcessStatsAndProfiles(e.Message);
			}
			else if (!(e.Message.SubscriptionId.StartsWith("user_") || e.Message.SubscriptionId.StartsWith("reply_")) && e.Message.MessageType == MessageTypeEnum.Eose)
			{
				var eventIds = NMessages.Where(m => m.SubscriptionId == e.Message.SubscriptionId).Select(m => m.Event.Id).ToList();
				var userIds = NMessages.Where(m => m.SubscriptionId == e.Message.SubscriptionId).Select(m => m.Event.Pubkey).ToList();
				_ = FetchReplies(eventIds, DateTime.Now);
				_ = FetchProfiles(userIds);
			}
			else if (!(e.Message.SubscriptionId.StartsWith("user_") || e.Message.SubscriptionId.StartsWith("reply_")) && e.Message.Event?.Kind == KindEnum.Text)
			{
				NMessages.Add(e.Message);
				StateUpdated.Invoke(this, null);
			}
		}
		private void ProcessStatsAndProfiles(NMessage message)
		{
			if (message.Event?.Kind == KindEnum.Metadata)
			{
				if (!string.IsNullOrEmpty(message.Event.Content))
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
						var eventsForUser = NMessages.Select(m => m.Event).Where(n => n.Pubkey == message.Event.Pubkey);
						foreach (var nEvent in eventsForUser)
						{
							nEvent.User = user;
						}
						Users.Add(user);
					}
					catch
					{

					}
				}
			}
			else if (message.SubscriptionId.StartsWith("reply_"))
			{
				var targetId = message.Event.ParentNEventId;
				NMessages.Select(m => m.Event).FirstOrDefault(n => n.Id == targetId).ChildNEvents.Add(message.Event);
			}
			StateUpdated.Invoke(this, null);
		}
		private async Task FetchProfiles(List<string> pubKeys)
		{
			var filter = new Filter
			{
				Kinds = new int[] { (int)KindEnum.Metadata },
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now,
				Authors = pubKeys.Distinct().ToList()
			};
			await FetchNEventsByFilter(MessageTypeEnum.Req, filter, $"user_{Guid.NewGuid()}");
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
