namespace BlazeJump.Common.Services.Notification
{
    public interface INotificationService
	{	
		event EventHandler? UpdateState;
		bool Loading { get; set; }
	}
}