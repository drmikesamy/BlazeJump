using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;

namespace BlazeJump.Client.Services.Message
{
    public interface IMessageService
    {
		event EventHandler<NMessage>? NewMessageProcessed;
        void AddMessage(NMessage nMessage);
        List<NEvent> GetAllMessages();
		Task FetchMessagesByFilter(Filter filter);
		Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId);
		Task SendNEvent(NEvent nEvent, string subscriptionHash);
	}
}