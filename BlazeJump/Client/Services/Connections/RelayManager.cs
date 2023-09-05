using BlazeJump.Client.Models;

namespace BlazeJump.Client.Services.Connections
{
	public class RelayManager : IRelayManager
	{
		public Dictionary<string, RelayConnection> RelayConnections { get; set; }
		public List<Task> ConnectionTasks { get; set; } = new List<Task>();
		public event EventHandler<NMessage>? NewMessageReceived;

		public RelayManager()
		{
			RelayConnections = new Dictionary<string, RelayConnection>();
		}

		public async Task OpenConnection(string uri)
		{
			RelayConnections.TryAdd(uri, new RelayConnection(uri));
			await RelayConnections[uri].ConnectAsync();
		}

		public void CloseConnection(string uri)
		{
			RelayConnections.TryGetValue(uri, out var connection);
			if (connection != null)
			{
				connection.Cancel();
			}
		}
		public async Task QueryRelays(List<string> uris, string subscriptionId, Filter filter)
		{
			foreach (var uri in uris)
			{
				Task connectionTask = Task.Run(async () =>
				{
					await OpenConnection(uri);
					RelayConnections[uri].NewMessageReceived += NewMessageReceived;
					await RelayConnections[uri].SubscribeAsync(subscriptionId, filter);
				});
				ConnectionTasks.Add(connectionTask);
			}
			await Task.WhenAll(ConnectionTasks);
		}

		public async Task SendNEvent(NEvent nEvent, List<string> uris, string subscriptionHash)
		{
			foreach (var uri in uris)
			{
				Task connectionTask = Task.Run(async () =>
				{
					await OpenConnection(uri);
					await RelayConnections[uri].SendNEvent(nEvent, subscriptionHash);
				});
				ConnectionTasks.Add(connectionTask);
			}
			await Task.WhenAll(ConnectionTasks);
		}
	}
}
