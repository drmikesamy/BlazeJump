using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using Newtonsoft.Json;
using BlazeJump.Common.Services.UserProfile;
using Microsoft.Maui.Controls;

namespace BlazeJump.Common.Services.Message
{
	public class RelationRegister
	{
		private Dictionary<string, Dictionary<KindEnum, List<string>>> _relationRegister = new();
		public void AddRelation(string parentEventId, KindEnum childEventKind, string childEventId)
		{
			if (!_relationRegister.ContainsKey(parentEventId))
			{
				_relationRegister.Add(parentEventId, new Dictionary<KindEnum, List<string>>());
			}
			if (!_relationRegister[parentEventId].ContainsKey(childEventKind))
			{
				_relationRegister[parentEventId].Add(childEventKind, new List<string>());
			}
			_relationRegister[parentEventId][childEventKind].Add(childEventId);
		}
		public IEnumerable<string> GetRelations(List<string> parentEventIds, KindEnum childEventKind)
		{
			foreach (var parentEventId in parentEventIds)
			{
				if (_relationRegister.TryGetValue(parentEventId, out var kindsAndChildEventIds)
				&& kindsAndChildEventIds.TryGetValue(childEventKind, out var childEventIds))
				{
					foreach(var childEventId in childEventIds)
					{
						yield return childEventId;
					}
				}
			}
		}
	}
	public class MessageService : IMessageService
	{
		private IRelayManager _relayManager;
		private ICryptoService _cryptoService;
		private IUserProfileService _userProfileService;
		public Dictionary<string, NMessage> MessageStore { get; set; } = new();
		public Dictionary<string, List<string>> TopLevelFetchRegister { get; set; } = new();
		public RelationRegister RelationRegister { get; set; } = new();
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
					MessageStore.TryAdd(message.Event.Id, message);
					if(TopLevelFetchRegister.TryGetValue(message.SubscriptionId, out var topLevelEventList))
					{
						topLevelEventList.Add(message.Event.Id);
					}
					ProcessRelations(message);
				}
			}
		}
		private void ProcessRelations(NMessage message)
		{
			RelationRegister.AddRelation(message.Event.Id, KindEnum.Metadata, message.Event.UserId);
			foreach (var tag in message.Event.Tags)
			{
				if(tag.Value != null)
				{
					switch (tag.Key)
					{
						case TagEnum.e:
							RelationRegister.AddRelation(tag.Value, KindEnum.Text, message.Event.Id);				
							break;
						case TagEnum.p:
							RelationRegister.AddRelation(message.Event.Id, KindEnum.Metadata, tag.Value);
							break;
					}
				}
			}
		}

		public async Task Fetch(List<Filter> filters, string? subscriptionId = null, MessageTypeEnum? messageType = null)
		{
			await _relayManager.QueryRelays(subscriptionId ?? Guid.NewGuid().ToString(), messageType ?? MessageTypeEnum.Req, filters);
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
