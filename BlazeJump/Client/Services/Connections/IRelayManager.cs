using BlazeJump.Client.Models;

namespace BlazeJump.Client.Services.Connections
{
    public interface IRelayManager
    {
        Dictionary<string, RelayConnection> RelayConnections { get; set; }
		Task OpenConnection(string uri);
        void CloseConnection(string uri);
		Task<List<string>> QueryRelays(List<string> uris, string subscriptionId, Filter filter, bool countOnly = false);
		Task SendNEvent(NEvent nEvent, List<string> uris, string subscriptionHash);
	}
}
