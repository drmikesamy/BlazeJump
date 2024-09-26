using BlazeJump.Common.Models;
using Newtonsoft.Json;
using BlazeJump.Common.Services.Connections.Events;
using System.Net.WebSockets;
using BlazeJump.Common.Enums;
using BlazeJump.Helpers;

namespace BlazeJump.Common.Services.Connections
{
	public class RelayManager : IRelayManager
	{
		public event EventHandler<MessageReceivedEventArgs> NewMessageReceived;
		public Dictionary<string, RelayConnection> RelayConnections { get; set; }
		public List<string> MessageQueue = new List<string>();

		public RelayManager()
		{
			var defaultRelay = "wss://nostr.wine";
			RelayConnections = new Dictionary<string, RelayConnection>();
			RelayConnections.TryAdd(defaultRelay, new RelayConnection(defaultRelay));
		}

		public List<string> OpenRelays => RelayConnections.Where(rc => rc.Value.WebSocket.State == WebSocketState.Open).Select(rc => rc.Key).ToList();

		public async Task OpenConnection(string uri)
		{
			if (RelayConnections[uri].WebSocket.State == WebSocketState.Open)
				return;

			var isFirstConnection = RelayConnections.TryAdd(uri, new RelayConnection(uri));
			await RelayConnections[uri].Init();
			Console.WriteLine("Event subscription");
			RelayConnections[uri].NewMessageReceived += NewMessageReceived;
		}

		public async Task CloseConnection(string uri)
		{
			RelayConnections.TryGetValue(uri, out var connection);
			if (connection != null)
			{
				await connection.Close();
			}
		}
		public async Task QueryRelays(List<string> uris, string subscriptionId, MessageTypeEnum requestMessageType, List<Filter> filters, int timeout = 15000)
		{
			var connectionTasks = new List<Task>();
			foreach (var uri in uris)
			{
				Task connectionTask = Task.Run(async () =>
				{
					await OpenConnection(uri);
					await RelayConnections[uri].SubscribeAsync(requestMessageType, subscriptionId, filters);
				});
				connectionTasks.Add(connectionTask);
			}
			await Task.WhenAll(connectionTasks);
		}

		public async Task SendNEvent(NEvent nEvent, List<string> uris, string subscriptionHash)
		{
			var connectionTasks = new List<Task>();
			object[] obj = { "EVENT", nEvent };
			string serialisedNMessage = JsonConvert.SerializeObject(obj);

			foreach (var uri in uris)
			{
				Task connectionTask = Task.Run(async () =>
				{
					await OpenConnection(uri);
					await RelayConnections[uri].SendNEvent(serialisedNMessage, subscriptionHash);
				});
				connectionTasks.Add(connectionTask);
			}
			await Task.WhenAll(connectionTasks);
		}
	}
}
