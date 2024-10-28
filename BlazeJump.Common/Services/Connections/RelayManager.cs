using BlazeJump.Common.Models;
using Newtonsoft.Json;
using BlazeJump.Common.Services.Connections.Events;
using BlazeJump.Common.Enums;
using System.Diagnostics;
using BlazeJump.Common.Services.Connections.Providers;

namespace BlazeJump.Common.Services.Connections
{
	public class RelayManager : IRelayManager
	{
		public Dictionary<string, IRelayConnection> RelayConnections { get; set; }
		public List<string> Relays => RelayConnections.Keys.ToList();
		public PriorityQueue<NMessage, Tuple<int, long>> ReceivedMessages { get; set; } = new PriorityQueue<NMessage, Tuple<int, long>>();
		public event EventHandler ProcessMessageQueue;
		private readonly IRelayConnectionProvider _connectionProvider;

		public RelayManager(IRelayConnectionProvider connectionProvider)
		{
			_connectionProvider = connectionProvider;
			RelayConnections = new Dictionary<string, IRelayConnection> {
				{ "wss://nostr.wine", _connectionProvider.CreateRelayConnection("wss://nostr.wine") }
			};
		}

		private void AddToQueue(object sender, MessageReceivedEventArgs e)
		{
			ReceivedMessages.Enqueue(e.Message, new Tuple<int, long>(0, Stopwatch.GetTimestamp()));
			ProcessMessageQueue?.Invoke(this, EventArgs.Empty);
		}

		public async Task OpenConnection(string uri)
		{
			if (RelayConnections.TryGetValue(uri, out IRelayConnection connection) && connection.IsOpen)
			{
				return;
			}
			IRelayConnection newConnection = _connectionProvider.CreateRelayConnection(uri);
			RelayConnections.TryAdd(uri, newConnection);
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

		public async Task SendNEvent(NEvent nEvent, string subscriptionHash)
		{
			var connectionTasks = new List<Task>();
			object[] obj = { "EVENT", nEvent };
			string serialisedNEvent = JsonConvert.SerializeObject(obj);

			foreach (var uri in Relays)
			{
				Task connectionTask = Task.Run(async () =>
				{
					await OpenConnection(uri);
					await RelayConnections[uri].SendNEvent(serialisedNEvent, subscriptionHash);
				});
				connectionTasks.Add(connectionTask);
			}
			await Task.WhenAll(connectionTasks);
		}
	}
}
