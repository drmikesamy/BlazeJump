using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Message;
using Newtonsoft.Json;
using BlazeJump.Common.Models.NostrConnect;

namespace BlazeJump.Common.Services.Identity
{
	public class IdentityService : IIdentityService
	{
		public event EventHandler<QrConnectEventArgs> QrConnectReceived;
		public PlatformEnum Platform { get; set; } = PlatformEnum.Web;
		private IRelayManager _relayManager {  get; set; }
		private IMessageService _messageService {  get; set; }
		public IdentityService(IRelayManager relayManager, IMessageService messageService) {
			_relayManager = relayManager;
			_messageService = messageService;
#if ANDROID
			Platform = PlatformEnum.Android;
#elif IOS
			Platform = PlatformEnum.Ios;
#else
			Platform = PlatformEnum.Web;
#endif
		}
		public void OnQrConnectReceived(QrConnectEventArgs e)
		{
			_ = SendNostrConnectReply(e.Pubkey, e.Secret);
		}
		private async Task SendNostrConnectReply(string theirPubKey, string secret)
		{
			var subscriptionHash = Guid.NewGuid().ToString();

			var message = new NostrConnectResponse
			{
				Id = subscriptionHash,
				Result = secret
			};

			var nEvent = _messageService.CreateNEvent(KindEnum.NostrConnect, JsonConvert.SerializeObject(message), null, null, new List<string> { theirPubKey });

			await _messageService.Send(KindEnum.NostrConnect, nEvent, theirPubKey);
		}
		public async Task FetchLoginScanResponse(QrConnectEventArgs payload)
		{
			var filter = new Filter
			{
				Kinds = new List<int> { (int)KindEnum.NostrConnect },
				Since = DateTime.Now.AddSeconds(-15),
				Until = DateTime.Now.AddSeconds(15),
				TaggedPublicKeys = new List<string> { payload.Pubkey }
			};

			var subscriptionHash = Guid.NewGuid().ToString();
			await _relayManager.QueryRelays(subscriptionHash, MessageTypeEnum.Req, new List<Filter> { filter }, 30000);
		}
	}
	public class QrConnectEventArgs : EventArgs
	{
		public string Relay { get; set; }
		public string Pubkey { get; set; }
		public string Secret { get; set; }
	}
}
