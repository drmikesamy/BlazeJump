using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;

namespace BlazeJump.Client.Services.Message
{
    public interface IMessageService
    {
		Task<List<NEvent>> FetchNEventsByFilter(Filter filter, bool fetchStats = false, bool fullFetch = false);
		Task<List<User>> FetchProfiles(List<string> pubKeys);
        List<NEvent> FetchMessagesFromDb(Func<NEvent, bool> selector);
        Task SendNEvent(NEvent nEvent, string subscriptionHash);
		Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId = null);
	}
}