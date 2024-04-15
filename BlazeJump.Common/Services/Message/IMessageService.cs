using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;

namespace BlazeJump.Common.Services.Message
{
	public interface IMessageService
	{
		Task<List<NEvent>> FetchNEventsByFilter(Filter filter, bool fetchStats = false, bool fullFetch = false);
		Task<List<User>> FetchProfiles(List<string> pubKeys);
		bool VerifyNEvent(NEvent nEvent);
		Task SendNEvent(KindEnum kind, string message);
	}
}