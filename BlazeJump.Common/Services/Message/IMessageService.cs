using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;

namespace BlazeJump.Common.Services.Message
{
	public interface IMessageService
	{
		event EventHandler StateUpdated;
		Dictionary<string, List<NMessage>> NMessages { get; set; }
		Dictionary<string, User> Users { get; set; }
		Task FetchNEventsByFilter(MessageTypeEnum requestMessageType, List<Filter> filters, string subscriptionId);
		bool VerifyNEvent(NEvent nEvent);
		Task SendNEvent(KindEnum kind, string message);
	}
}