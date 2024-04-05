using BlazeJump.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazeJump.Common.Services.Identity
{
	public interface IIdentityService
	{
		event EventHandler<QrConnectEventArgs> QrConnectReceived;
		void OnQrConnectReceived(QrConnectEventArgs e);
		PlatformEnum Platform { get; set; }
	}
}
