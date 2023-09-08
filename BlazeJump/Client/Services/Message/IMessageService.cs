using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;

namespace BlazeJump.Client.Services.Message
{
    public interface IMessageService
    {
		event EventHandler<NMessage>? NewMessageProcessed;
        void AddMessage(NMessage nMessage);
		Task FetchMessagesByFilter(Filter filter, int parentDepth = 1);
		Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId);
		Task SendNEvent(NEvent nEvent, string subscriptionHash);
	}
}