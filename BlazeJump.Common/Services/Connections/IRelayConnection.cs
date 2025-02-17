using System.Collections.Concurrent;
using System.Net.WebSockets;
using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;

namespace BlazeJump.Common.Services.Connections;

public interface IRelayConnection
{
    bool IsOpen { get; }
	ConcurrentDictionary<string, bool> ActiveSubscriptions { get; set; }
    event EventHandler<MessageReceivedEventArgs> NewMessageReceived;
    Task Init();
    Task SubscribeAsync(MessageTypeEnum requestMessageType, string subscriptionId, List<Filter> filters);
    Task SendNEvent(string serialisedNMessage, string subscriptionId);
    Task Close();
}