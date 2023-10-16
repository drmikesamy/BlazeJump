using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;

namespace BlazeJump.Common.Services.Message
{
    public interface IMessageService
    {
		Task<List<NEvent>> FetchNEventsByFilter(Filter filter, bool fetchStats = false, bool fullFetch = false);
		Task<List<User>> FetchProfiles(List<string> pubKeys);
        List<NEvent> FetchMessagesFromDb(Func<NEvent, bool> selector);
#if ANDROID
		Task SendNEvent(NEvent nEvent, string subscriptionHash);
#endif
		Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId = null);
	}
}