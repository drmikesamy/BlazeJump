using BlazeJump.Common.Models;
using Newtonsoft.Json;
using BlazeJump.Common.Services.Connections.Events;
using BlazeJump.Common.Enums;
using System.Diagnostics;
using BlazeJump.Common.Services.Connections.Providers;
using System.Threading;
using System.Collections.Concurrent;

namespace BlazeJump.Common.Services.Connections
{
	public class RelayManager : IRelayManager
	{
		public ConcurrentDictionary<string, IRelayConnection> RelayConnections { get; set; } = new ConcurrentDictionary<string, IRelayConnection>();
		public List<string> Relays => RelayConnections.Keys.ToList();
		public ConcurrentQueue<NMessage> ReceivedMessages { get; set; } = new ConcurrentQueue<NMessage>();
		public event EventHandler ProcessMessageQueue;
		private readonly IRelayConnectionProvider _connectionProvider;
		private readonly object _processMessageQueueLock = new object();

		public RelayManager(IRelayConnectionProvider connectionProvider)
		{
			_connectionProvider = connectionProvider;
			RelayConnections.TryAdd("wss://relay.nostr.band", _connectionProvider.CreateRelayConnection("wss://relay.nostr.band"));
		}

		private void AddToQueue(object sender, MessageReceivedEventArgs e)
		{
			Console.WriteLine($"Adding Event {e.Message?.Event?.Id} to queue");
			ReceivedMessages.Enqueue(e.Message);
			lock (_processMessageQueueLock)
			{
				ProcessMessageQueue?.Invoke(this, EventArgs.Empty);
			}
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
			if (RelayConnections.TryRemove(uri, out var connection) && connection != null)
			{
				await connection.Close();
			}
		}

		public async Task QueryRelays(string subscriptionId, MessageTypeEnum requestMessageType, List<Filter> filters, int timeout = 15000)
		{
			using var resource = new SemaphoreSlim(5, 5);

			var connectionTasks = Relays.Select(async uri =>
			{
				await resource.WaitAsync(timeout);
				try
				{
					await OpenConnection(uri);
					await RelayConnections[uri].SubscribeAsync(requestMessageType, subscriptionId, filters);
				}
				catch
				{
					Console.WriteLine($"Failed to connect to relay {uri}");
				}
				finally
				{
					resource.Release();
				}
			});
			await Task.WhenAll(connectionTasks);
		}

		public async Task SendNEvent(NEvent nEvent, string subscriptionHash)
		{
			using var resource = new SemaphoreSlim(5, 5);

			object[] obj = { "EVENT", nEvent };
			string serialisedNEvent = JsonConvert.SerializeObject(obj);

			var connectionTasks = Relays.Select(async uri =>
			{
				await resource.WaitAsync();
				try
				{
					await OpenConnection(uri);
					await RelayConnections[uri].SendNEvent(serialisedNEvent, subscriptionHash);
				}
				catch
				{
					Console.WriteLine($"Failed to connect to relay {uri}");
				}
				finally
				{
					resource.Release();
				}
			});
			await Task.WhenAll(connectionTasks);
		}
	}
}
