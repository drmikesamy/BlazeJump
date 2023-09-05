using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;
using BlazeJump.Client.Pages;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;

namespace BlazeJump.Client.Services.Connections
{
	public class RelayConnection
	{
		public CancellationTokenSource _webSocketCancellationTokenSource;
		public CancellationToken _webSocketCancellationToken;
		public CancellationTokenSource _listenerCancellationTokenSource;
		public CancellationToken _listenerCancellationToken;
		private readonly string _uri;
		private Dictionary<string, bool> activeSubscriptions { get; set; }
		public ClientWebSocket WebSocket { get; set; }
		public List<string> Messages { get; set; }
		public event EventHandler<NMessage>? NewMessageReceived;
		public RelayConnection(string uri)
		{
			_uri = uri;
			activeSubscriptions= new Dictionary<string, bool>();
			_webSocketCancellationTokenSource = new CancellationTokenSource();
			_webSocketCancellationToken = _webSocketCancellationTokenSource.Token;
			_listenerCancellationTokenSource = new CancellationTokenSource();
			_listenerCancellationToken = _listenerCancellationTokenSource.Token;
			Messages = new List<string>();
			WebSocket = new ClientWebSocket();
		}
		public async Task ConnectAsync()
		{
			if (WebSocket.State == WebSocketState.Open)
			{
				Console.WriteLine($"Already connected to relay: {_uri}.");
				return;
			}
			try
			{
				await WebSocket.ConnectAsync(new Uri(_uri), _webSocketCancellationToken);
				_ = MessageLoop();
				//await BasicSubscription();
			}
			catch
			{
				Console.WriteLine($"Failed to connect to relay: {_uri}.");
			}
		}

		private async Task MessageLoop()
		{
			Console.WriteLine($"setting up listener for {_uri}");
			await foreach (var rawMessage in ReceiveLoop())
			{
				if (rawMessage == null || rawMessage == "")
				{
					continue;
				}
				Console.WriteLine($"Message received from {_uri}");

				try
				{
					var nMessage = JsonConvert.DeserializeObject<NMessage>(rawMessage);
					if(nMessage != null)
						_ = HandleMessage(nMessage);
				}
				catch (Exception e)
				{
					var logMessageForException = rawMessage == null ? "null" : rawMessage;
					Console.WriteLine($"Incompatible message type in message: {logMessageForException}");
					Console.WriteLine($"Exception message: {e.Message}");
				}
			}
		}

		private async Task HandleMessage(NMessage nMessage)
		{
			NewMessageReceived?.Invoke(this, nMessage);
			if (nMessage.SubscriptionId != null && nMessage!.MessageType == MessageTypeEnum.Eose)
			{
				await UnSubscribeAsync(nMessage.SubscriptionId);
			}
			
		}

		private async IAsyncEnumerable<string> ReceiveLoop()
		{
			var buffer = new ArraySegment<byte>(new byte[2048]);
			while (true)
			{
				WebSocketReceiveResult result;
				using var ms = new MemoryStream();
				do
				{
					result = await WebSocket.ReceiveAsync(buffer, _webSocketCancellationToken);
					ms.Write(buffer.Array!, buffer.Offset, result.Count);
				} while (!result.EndOfMessage);

				ms.Seek(0, SeekOrigin.Begin);

				yield return Encoding.UTF8.GetString(ms.ToArray());

				if (result.MessageType == WebSocketMessageType.Close || _listenerCancellationToken.IsCancellationRequested)
				{
					_listenerCancellationTokenSource.Dispose();
					_listenerCancellationTokenSource = new CancellationTokenSource();
					_listenerCancellationToken = _listenerCancellationTokenSource.Token;
					break;
				}
			}
		}
		public async Task SubscribeAsync(string subscriptionId, Filter filter)
		{
			if (WebSocket.State == WebSocketState.Open)
			{
				Console.WriteLine($"subscribing to {_uri} using {subscriptionId}");

				object[] obj = { "REQ", subscriptionId, filter };

				string newsub = JsonConvert.SerializeObject(obj);

				var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(newsub));

				await WebSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _webSocketCancellationToken);

				activeSubscriptions.TryAdd(subscriptionId, true);
			}
		}
		public async Task UnSubscribeAsync(string subscriptionId)
		{
			if (WebSocket.State == WebSocketState.Open)
			{
				Console.WriteLine($"unsubscribing from {_uri}");
				object[] obj = { "CLOSE", subscriptionId };

				string closeMessage = JsonConvert.SerializeObject(obj);

				var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(closeMessage));

				await WebSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _webSocketCancellationToken);
				activeSubscriptions.Remove(subscriptionId);
			}
		}
		public async Task SendNEvent(NEvent nEvent, string subscriptionId)
		{
			if (WebSocket.State == WebSocketState.Open)
			{
				Console.WriteLine($"Sending NEvent to {_uri} using {subscriptionId}");

				object[] obj = { "EVENT", nEvent };

				string serialisedNMessage = JsonConvert.SerializeObject(obj);

				var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(serialisedNMessage));

				await WebSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _webSocketCancellationToken);
			}
		}
		public async void Cancel()
		{
			var unsubscribeTasks = new List<Task>();

			foreach (var activeSubscription in activeSubscriptions)
			{
				Task connectionTask = Task.Run(async () =>
				{
					await UnSubscribeAsync(activeSubscription.Key);
				});
				unsubscribeTasks.Add(connectionTask);
			}
			await Task.WhenAll(unsubscribeTasks);

			_listenerCancellationTokenSource.Cancel();
			WebSocket.Dispose();
			WebSocket = new ClientWebSocket();
		}
	}
}
