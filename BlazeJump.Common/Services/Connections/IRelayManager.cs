﻿using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;
using BlazeJump.Common.Enums;

namespace BlazeJump.Common.Services.Connections
{
    public interface IRelayManager
    {
		event EventHandler ProcessMessageQueue;
		PriorityQueue<NMessage, Tuple<int, long>> ReceivedMessages { get; set; }
		List<string> Relays { get; }
		Dictionary<string, IRelayConnection> RelayConnections { get; set; }
		Task OpenConnection(string uri);
		Task CloseConnection(string uri);
		Task QueryRelays(string subscriptionId, MessageTypeEnum requestMessageType, List<Filter> filters, int timeout = 15000);
		Task SendNEvent(NEvent nEvent, string subscriptionHash);
	}
}
