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
		private Dictionary<string, NEvent> sendMessageQueue = new Dictionary<string, NEvent>();
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public MessageService(IRelayManager relayManager, ICryptoService cryptoService, IUserProfileService userProfileService)
		{
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_userProfileService = userProfileService;
			_relayManager.NewMessageReceived += RelayMessageReceived;
		}
		public void RelayMessageReceived(object sender, MessageReceivedEventArgs e)
		{
			MessageReceived?.Invoke(sender, e);
		}
		public async Task Fetch(MessageTypeEnum requestMessageType, List<Filter> filters, string subscriptionId)
		{
			await _relayManager.OpenConnection("wss://nostr.wine");
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
			var verified = _cryptoService.Verify(nEvent.Sig, serialisedNEvent, nEvent.Pubkey);
			return verified;
		}
		public async Task Send(KindEnum kind, string message)
		{
			var nEvent = CreateNEvent(kind, message);
			var subscriptionHash = Guid.NewGuid().ToString();
			Sign(ref nEvent);
			await _relayManager.SendNEvent(nEvent, _relayManager.OpenRelays, subscriptionHash);
			sendMessageQueue.TryAdd(nEvent.Id, nEvent);
		}
		private NEvent CreateNEvent(KindEnum kind, string message, string parentId = null)
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
