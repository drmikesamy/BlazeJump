using BlazeJump.Common.Models;

namespace BlazeJump.Common.Services.Connections
{
    public interface IRelayManager
    {
        Dictionary<string, RelayConnection> RelayConnections { get; set; }
		Task OpenConnection(string uri);
        void CloseConnection(string uri);
		Task<List<string>> QueryRelays(List<string> uris, string subscriptionId, Filter filter, int timeout = 15000);
		Task SendNEvent(NEvent nEvent, List<string> uris, string subscriptionHash);
	}
}
