using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;

namespace BlazeJump.Client.Services.Message
{
    public interface IMessageService
    {
		Task<List<NEvent>> FetchNEventsByFilter(Filter filter, bool fullFetch = false);
        Task<NEvent?> FetchById(string nEventId);
        Task<List<NEvent>> FetchNEventsByParentId(string nEventId);
        List<NEvent> FetchMessagesFromDb(Func<NEvent, bool> selector);
        Task SendNEvent(NEvent nEvent, string subscriptionHash);
		Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId = null);
	}
}