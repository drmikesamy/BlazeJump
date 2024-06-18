using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;

namespace BlazeJump.Common.Services.Message
{
	public interface IMessageService
	{
		event EventHandler<MessageReceivedEventArgs> NewMessageReceived;
		Task FetchNEventsByFilter(MessageTypeEnum requestMessageType, List<Filter> filters, string subscriptionId);
		bool VerifyNEvent(NEvent nEvent);
		Task SendNEvent(KindEnum kind, string message);
		void RelayMessageReceived(object sender, MessageReceivedEventArgs e);
	}
}