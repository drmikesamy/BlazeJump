using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;
using BlazeJump.Common.Enums;
using System.Collections.Concurrent;

namespace BlazeJump.Common.Services.Connections
{
    public interface IRelayManager
    {
		event EventHandler ProcessMessageQueue;
		ConcurrentQueue<NMessage> ReceivedMessages { get; set; }
		List<string> Relays { get; }
		Dictionary<string, IRelayConnection> RelayConnections { get; set; }
		Task OpenConnection(string uri);
		Task CloseConnection(string uri);
		Task QueryRelays(string subscriptionId, MessageTypeEnum requestMessageType, List<Filter> filters, int timeout = 15000);
		Task SendNEvent(NEvent nEvent, string subscriptionHash);
	}
}
