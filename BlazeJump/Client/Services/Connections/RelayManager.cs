using BlazeJump.Client.Models;
using Newtonsoft.Json;

namespace BlazeJump.Client.Services.Connections
{
	public class RelayManager : IRelayManager
	{
		public Dictionary<string, RelayConnection> RelayConnections { get; set; }
		public List<string> MessageQueue = new List<string>();

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
		public async Task<List<string>> QueryRelays(List<string> uris, string subscriptionId, Filter filter)
		{
			var connectionTasks = new List<Task>();

			var messages = new List<string>();

			foreach (var uri in uris)
			{
				Task connectionTask = Task.Run(async () =>
				{
					await OpenConnection(uri);
					await RelayConnections[uri].SubscribeAsync(subscriptionId, filter);
					var connectionMessages = await RelayConnections[uri].MessageLoop();
					if(connectionMessages != null)
						messages.AddRange(connectionMessages);
				});
				connectionTasks.Add(connectionTask);
			}
			await Task.WhenAll(connectionTasks);
			return messages;
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
