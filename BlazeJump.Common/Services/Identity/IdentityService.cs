﻿using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Message;
using Newtonsoft.Json;
using BlazeJump.Common.Models.NostrConnect;

namespace BlazeJump.Common.Services.Identity
{
	public class IdentityService : IIdentityService
	{
		private bool _isBusy { get; set; } = false;
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
			if(_isBusy) return;
			_isBusy = true;
			_ = SendNostrConnectReply(e.Pubkey);
		}
		private async Task SendNostrConnectReply(string theirPubKey)
		{
			var subscriptionHash = Guid.NewGuid().ToString();
			var message = new NostrConnectResponse
			{
				Id = subscriptionHash,
				Result = "Connected"
			};
			await _messageService.Send(KindEnum.NostrConnect, JsonConvert.SerializeObject(message));
			_isBusy = false;
		}
		public async Task FetchLoginScanResponse(QrConnectEventArgs payload)
		{
			var filter = new Filter
			{
				Kinds = new int[] { (int)KindEnum.NostrConnect },
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
	}
}
