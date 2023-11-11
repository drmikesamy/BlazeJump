using BlazeJump.Common.Enums;

namespace BlazeJump.Common.Services.Notification
{
	public class NotificationService : INotificationService
	{
		public event EventHandler<DeepLinkReceivedEventArgs> DeepLinkReceived;
		public void OnDeepLinkReceived(string data)
		{
			PreviousDeepLink = data;
			if(DeepLinkReceived != null)
			{
                DeepLinkReceived.Invoke(this, new DeepLinkReceivedEventArgs() { Data = data });
            }
		}
		public string? PreviousDeepLink {  get; set; }
		public event EventHandler? UpdateState;
		private bool _loading { get; set; } = false;
		public PlatformEnum Platform { get; set; } = PlatformEnum.Web;
		public NotificationService()
		{
#if ANDROID
			Platform = PlatformEnum.Android;
#elif IOS
			Platform = PlatformEnum.Ios;
#endif
		}
		public bool Loading
		{
			get
			{
				return _loading;
			}
			set
			{
				if (_loading != value)
				{
					_loading = value;
					UpdateState?.Invoke(this, EventArgs.Empty);
				}
			}
		}
	}
	public class DeepLinkReceivedEventArgs : EventArgs
	{
		public required string Data { get; set; }
	}
}