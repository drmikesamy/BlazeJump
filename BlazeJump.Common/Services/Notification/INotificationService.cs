using BlazeJump.Common.Enums;

namespace BlazeJump.Common.Services.Notification
{
    public interface INotificationService
    {
		event EventHandler<DeepLinkReceivedEventArgs>? DeepLinkReceived;
		string? PreviousDeepLink { get; set; }
		void OnDeepLinkReceived(string data);
		event EventHandler? UpdateState;
		bool Loading { get; set; }
		PlatformEnum Platform { get; set; }
	}
}