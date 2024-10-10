using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;

namespace BlazeJump.Common.Services.Message
{
	public interface IMessageService
	{
		event EventHandler<string> EndOfFetchNotification;
		Dictionary<string, List<NMessage>> ReceivedMessages { get; set; }
		Dictionary<string, string> EventSubscriptionIds { get; set; }
		Dictionary<string, User> UserStore { get; set; }
		Task Fetch(MessageTypeEnum requestMessageType, List<Filter> filters, string subscriptionId);
		bool Verify(NEvent nEvent);
		Task Send(KindEnum kind, string message);
	}
}