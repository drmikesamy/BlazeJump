using BlazeJump.Client.Models;

namespace BlazeJump.Client.Services.Connections
{
    public interface IRelayManager
    {
        Dictionary<string, RelayConnection> RelayConnections { get; set; }
		event EventHandler<NMessage>? NewMessageReceived;
        Task OpenConnection(string uri);
        void CloseConnection(string uri);
		List<Task> ConnectionTasks { get; set; }
        Task QueryRelays(List<string> uris, string subscriptionId, Filter filter);
		Task SendNEvent(NEvent nEvent, List<string> uris, string subscriptionHash);
	}
}
