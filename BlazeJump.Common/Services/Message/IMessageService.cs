using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;

namespace BlazeJump.Common.Services.Message
{
	public interface IMessageService
	{
		event EventHandler<MessageReceivedEventArgs> MessageReceived;
		Task Fetch(MessageTypeEnum requestMessageType, List<Filter> filters, string subscriptionId);
		bool Verify(NEvent nEvent);
		Task Send(KindEnum kind, string message);
		void RelayMessageReceived(object sender, MessageReceivedEventArgs e);
	}
}