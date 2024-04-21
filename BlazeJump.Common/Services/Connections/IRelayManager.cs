﻿using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;
using BlazeJump.Common.Enums;

namespace BlazeJump.Common.Services.Connections
{
    public interface IRelayManager
    {
		event EventHandler<MessageReceivedEventArgs> NewMessageReceived;
		Dictionary<string, RelayConnection> RelayConnections { get; set; }
		Task OpenConnection(string uri);
		Task CloseConnection(string uri);
		Task QueryRelays(List<string> uris, string subscriptionId, MessageTypeEnum requestMessageType, Filter filter, int timeout = 15000);
		Task SendNEvent(NEvent nEvent, List<string> uris, string subscriptionHash);
	}
}
