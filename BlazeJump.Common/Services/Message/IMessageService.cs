using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;

namespace BlazeJump.Common.Services.Message
{
	public interface IMessageService
	{
		event EventHandler<string> EndOfFetchNotification;
		RelationRegister RelationRegister { get; set; }
		Dictionary<string, NMessage> MessageStore { get; set; }
		Dictionary<string, List<string>> TopLevelFetchRegister { get; set; }
		Task Fetch(List<Filter> filters, string? subscriptionId = null, MessageTypeEnum? messageType = null);
		bool Verify(NEvent nEvent);
		Task Send(KindEnum kind, string message);
	}
}