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
		public event EventHandler<MessageReceivedEventArgs> NewMessageReceived;

		public MessageService(IRelayManager relayManager, ICryptoService cryptoService, IUserProfileService userProfileService)
		{
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_userProfileService = userProfileService;
			_relayManager.NewMessageReceived += RelayMessageReceived;
		}
		public void RelayMessageReceived(object sender, MessageReceivedEventArgs e)
		{
			NewMessageReceived?.Invoke(sender, e);
		}
		public async Task FetchNEventsByFilter(MessageTypeEnum requestMessageType, List<Filter> filters, string subscriptionId)
		{
			await _relayManager.QueryRelays(new List<string> { "wss://nostr.wine" }, subscriptionId, requestMessageType, filters);
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
