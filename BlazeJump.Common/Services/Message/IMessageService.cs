using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;

namespace BlazeJump.Common.Services.Message
{
	public interface IMessageService
	{
		event EventHandler StateUpdated;
		List<NEvent> NEvents { get; }
		List<NMessage> NMessages { get; set; }
		List<User> Users { get; set; }
		Task FetchUserPage(string pubKey);
		Task FetchEventPage(string eventId);
		Task FetchReplies(List<string> eventIds, DateTime cutOffDate);
		Task FetchNEventsByFilter(MessageTypeEnum requestMessageType, Filter filter, string subscriptionId = null);
		bool VerifyNEvent(NEvent nEvent);
		Task SendNEvent(KindEnum kind, string message);
	}
}