using BlazeJump.Common.Models;
using Newtonsoft.Json;
using BlazeJump.Common.Services.Connections.Events;
using System.Net.WebSockets;
using BlazeJump.Common.Enums;
using System.Diagnostics;

namespace BlazeJump.Common.Services.Connections
{
	public class RelayManager : IRelayManager
	{
		public Dictionary<string, RelayConnection> RelayConnections { get; set; }
		public List<string> Relays => RelayConnections.Keys.ToList();
		public PriorityQueue<NMessage, Tuple<int, long>> ReceivedMessages { get; set; } = new PriorityQueue<NMessage, Tuple<int, long>>();

		public RelayManager()
		{
			RelayConnections = new Dictionary<string, RelayConnection> {
				{ "wss://nostr.wine", new RelayConnection("wss://nostr.wine") }
			};
		}

		private void AddToQueue(object sender, MessageReceivedEventArgs e)
		{
			ReceivedMessages.Enqueue(e.Message, new Tuple<int, long>(e.Message.Priority, Stopwatch.GetTimestamp()));
		}

		public async Task OpenConnection(string uri)
		{
			if (RelayConnections.TryGetValue(uri, out var value) && value.WebSocket.State == WebSocketState.Open)
				return;
			RelayConnections.TryAdd(uri, new RelayConnection(uri));
			await RelayConnections[uri].Init();
			Console.WriteLine("Event subscription");
			RelayConnections[uri].NewMessageReceived += AddToQueue;
		}

		public async Task CloseConnection(string uri)
		{
			RelayConnections.TryGetValue(uri, out var connection);
			if (connection != null)
			{
				await connection.Close();
			}
		}
		public async Task QueryRelays(string subscriptionId, MessageTypeEnum requestMessageType, List<Filter> filters, int timeout = 15000)
		{
			var connectionTasks = new List<Task>();
			foreach (var uri in Relays)
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
