using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;

namespace BlazeJump.Common.Services.Identity
{
	public interface IIdentityService
	{
		event EventHandler<QrConnectEventArgs> QrConnectReceived;
		void OnQrConnectReceived(QrConnectEventArgs e);
		Task<List<NEvent>> FetchLoginScanResponse(QrConnectEventArgs payload);
		PlatformEnum Platform { get; set; }
	}
}
