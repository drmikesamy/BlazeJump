using BlazeJump.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazeJump.Common.Services.Identity
{
	public class IdentityService : IIdentityService
	{
		public event EventHandler<QrConnectEventArgs> QrConnectReceived;
		public PlatformEnum Platform { get; set; } = PlatformEnum.Web;
		public IdentityService() {
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
			throw new NotImplementedException();
		}
	}
	public class QrConnectEventArgs : EventArgs
	{
		public string Relay { get; set; }
		public string Pubkey { get; set; }
	}
}
