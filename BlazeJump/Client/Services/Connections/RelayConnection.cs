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
		public Dictionary<string, bool> ActiveSubscriptions { get; set; }
		public ClientWebSocket WebSocket { get; set; }
		public List<string> Messages { get; set; }
		public event EventHandler<string>? NewMessageReceived;
		public RelayConnection(string uri)
		{
			_uri = uri;
			ActiveSubscriptions = new Dictionary<string, bool>();
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
			}
			catch
			{
				Console.WriteLine($"Failed to connect to relay: {_uri}.");
			}
		}
		public async Task SubscribeAsync(string subscriptionId, Filter filter)
		{
			if (WebSocket.State == WebSocketState.Open)
			{
				Console.WriteLine($"subscribing to {_uri} using {subscriptionId}");
				await SendRequest(MessageTypeEnum.Req, subscriptionId, filter);
                ActiveSubscriptions.TryAdd(subscriptionId, true);
			}
		}
		public async Task SendRequest(MessageTypeEnum requestMessageType, string subscriptionId, Filter filter)
		{
            object[] obj = { requestMessageType.ToString().ToUpper(), subscriptionId, filter };

            string newsub = JsonConvert.SerializeObject(obj);

            var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(newsub));

            await WebSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _webSocketCancellationToken);
        }
		public async Task<List<string>> MessageLoop(bool countOnly = false)
		{
			var messages = new List<string>();
			if(countOnly)
				messages.Add("0");
			var messageCount = 0;
			Console.WriteLine($"setting up listener for {_uri}");
			await foreach (var rawMessage in ReceiveLoop())
			{
				if (rawMessage == null || rawMessage == "")
				{
					continue;
				}
				Console.WriteLine($"Message received from {_uri}");
				if (!rawMessage.StartsWith("[\"EVENT"))
				{
					var abbreviatedRawMessage = rawMessage.Count() < 100 ? rawMessage : rawMessage.Substring(0, 100);

					var subscriptionId = ActiveSubscriptions.Keys.FirstOrDefault(s => abbreviatedRawMessage.Contains(s));

					if (subscriptionId != null){
						await UnSubscribeAsync(subscriptionId);
						break;
					}
				}
				else
				{
                    if (countOnly)
                    {
                        messageCount++;
                        messages[0] = messageCount.ToString();
                    }
                    else
                    {
                        messages.Add(rawMessage);
                    }
                }
			}
			return messages;
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
		public async Task SendNEvent(string serialisedNMessage, string subscriptionId)
		{
			if (WebSocket.State == WebSocketState.Open)
			{
				Console.WriteLine($"Sending NEvent to {_uri} using {subscriptionId}");

				var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(serialisedNMessage));

				await WebSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _webSocketCancellationToken);
			}
		}
		public async Task UnSubscribeAsync(string subscriptionId)
		{
			if (WebSocket.State == WebSocketState.Open)
			{
				Console.WriteLine($"unsubscribing from {_uri}, subscriptionid {subscriptionId}");
				object[] obj = { "CLOSE", subscriptionId };

				string closeMessage = JsonConvert.SerializeObject(obj);

				var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(closeMessage));

				await WebSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _webSocketCancellationToken);
				ActiveSubscriptions.Remove(subscriptionId);
			}
		}
		public async void Cancel()
		{
			var unsubscribeTasks = new List<Task>();

			foreach (var activeSubscription in ActiveSubscriptions)
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
