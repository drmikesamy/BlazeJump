using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using Newtonsoft.Json;
using BlazeJump.Common.Services.UserProfile;

namespace BlazeJump.Common.Services.Message
{
	public class MessageService : IMessageService
	{
		private IRelayManager _relayManager;
		private ICryptoService _cryptoService;
		private IUserProfileService _userProfileService;
		private Dictionary<string, NEvent> sendMessageQueue = new();
		public Dictionary<string, List<string>> SubscriptionIdToEventIdList { get; set; } = new();
		public Dictionary<string, NMessage> MessageStore { get; set; } = new();
		public Dictionary<string, string> EventIdToSubscriptionId { get; set; } = new();
		public Dictionary<string, User> UserStore { get; set; } = new();
		public event EventHandler<string> EndOfFetchNotification;
		public MessageService(IRelayManager relayManager, ICryptoService cryptoService, IUserProfileService userProfileService)
		{
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_userProfileService = userProfileService;
			_ = ProcessReceivedMessages();
		}
		private async Task ProcessReceivedMessages()
		{
			while (true)
			{
				await Task.Delay(1);
				if (_relayManager.ReceivedMessages.Count > 0)
				{
					Console.WriteLine($"{_relayManager.ReceivedMessages.Count} messages in receive queue.");
					var message = _relayManager.ReceivedMessages.Dequeue();

					if (message.MessageType == MessageTypeEnum.Eose)
					{
						Console.WriteLine($"EOSE message received. Refreshing view.");
						EndOfFetchNotification?.Invoke(this, message.SubscriptionId);
						continue;
					}

					if (message.Event == null)
					{
						Console.WriteLine($"Message has no event. Skipping.");
						continue;
					}

					Console.WriteLine($"Processing {message.MessageType} message with id {message.Event.Id}");

					SubscriptionIdToEventIdList.TryAdd(message.SubscriptionId, new List<string>());
					MessageStore.TryAdd(message.Event.Id, message);
					SubscriptionIdToEventIdList[message.SubscriptionId].Add(message.Event.Id);
					EventIdToSubscriptionId.TryAdd(message.Event.Id, message.SubscriptionId);

					if (message.Event.Kind == KindEnum.Metadata)
					{
						Console.WriteLine($"Adding user {message.Event.User.Username} to store.");
						UserStore.TryAdd(message.Event.UserId, message.Event.User);
					}
					if (message.Event.Tags != null && message.Event?.Tags.Count > 0)
					{
						var root = message.Event.Tags?.FirstOrDefault(t => t.Key == TagEnum.e && t.Value3 == "root");
						var reply = message.Event.Tags?.FirstOrDefault(t => t.Key == TagEnum.e && t.Value3 == "reply");
						if (root != null
							&& MessageStore.TryGetValue(root.Value, out var parentMessage))
						{
								Console.WriteLine($"Adding event {message.Event.Id} to parent event {parentMessage.Event.Id}.");
								parentMessage.Event.Replies.TryAdd(message.Event.Id, message.Event);
						}
						if (reply != null
							&& MessageStore.TryGetValue(reply.Value, out var parentMessagereply))
						{
								Console.WriteLine($"Adding event {message.Event.Id} to parent event {parentMessagereply.Event.Id}.");
								parentMessagereply.Event.Replies.TryAdd(message.Event.Id, message.Event);
						}
					}
				}
			}
		}
		public async Task Fetch(MessageTypeEnum requestMessageType, List<Filter> filters, string subscriptionId)
		{
			await _relayManager.QueryRelays(subscriptionId, requestMessageType, filters);
		}
		private bool Sign(ref NEvent nEvent)
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
		public bool Verify(NEvent nEvent)
		{
			var signableEvent = nEvent.GetSignableNEvent();
			var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
			var verified = _cryptoService.Verify(nEvent.Sig, serialisedNEvent, nEvent.UserId);
			return verified;
		}
		public async Task Send(KindEnum kind, string message)
		{
			var nEvent = CreateNEvent(kind, message);
			var subscriptionHash = Guid.NewGuid().ToString();
			Sign(ref nEvent);
			await _relayManager.SendNEvent(nEvent, _relayManager.Relays, subscriptionHash);
			sendMessageQueue.TryAdd(nEvent.Id, nEvent);
		}
		private NEvent CreateNEvent(KindEnum kind, string message, string parentId = null)
		{
			return new NEvent
			{
				Id = "0",
				UserId = _userProfileService.NPubKey,
				Kind = kind,
				Content = message,
				Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			};
		}
	}
}
