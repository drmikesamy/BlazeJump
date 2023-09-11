using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;

namespace BlazeJump.Client.Services.Message
{
    public interface IMessageService
    {
		Task<List<NEvent>> FetchMessagesByFilter(Filter filter);
		Task SendNEvent(NEvent nEvent, string subscriptionHash);
		Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId = null);
	}
}