using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using BlazeJump.Common.Services.Connections.Factories;

namespace BlazeJump.Common.Services.Connections
{
    public class RelayConnection : IRelayConnection
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly string _uri;
        private IClientWebSocketWrapper _webSocket { get; set; }
        private IClientWebSocketFactory _webSocketFactory { get; set; }
        public Dictionary<string, bool> ActiveSubscriptions { get; set; } = new Dictionary<string, bool>();
        public event EventHandler<MessageReceivedEventArgs> NewMessageReceived;
        public bool IsOpen => _webSocket is { State: WebSocketState.Open };

        public RelayConnection(IClientWebSocketFactory webSocketFactory, string uri)
        {
            _webSocketFactory = webSocketFactory;
            _webSocket = _webSocketFactory.Create();
            _uri = uri;
        }

        public async Task Init()
        {
            await ConnectAsync();
            _ = MessageLoop();
        }

        private async Task ConnectAsync()
        {
            if (IsOpen)
            {
                Console.WriteLine($"Already connected to relay: {_uri}.");
                return;
            }
            else if (_webSocket.State == WebSocketState.Aborted)
            {
                _webSocket.Dispose();
            }

            _webSocket = _webSocketFactory.Create();
            await _webSocket.ConnectAsync(new Uri(_uri), _cancellationTokenSource.Token);
        }

        public async Task SubscribeAsync(MessageTypeEnum requestMessageType, string subscriptionId,
            List<Filter> filters)
        {
            if (IsOpen)
            {
                if (ActiveSubscriptions.ContainsKey(subscriptionId))
                {
                    Console.WriteLine($"Already subscribed to {_uri} using {subscriptionId}");
                }
                else
                {
                    Console.WriteLine($"Subscribing to {_uri} using {subscriptionId}");
                    await SendRequest(requestMessageType, subscriptionId, filters);
                    ActiveSubscriptions.TryAdd(subscriptionId, true);
                }
            }
        }

        private async Task SendRequest(MessageTypeEnum requestMessageType, string subscriptionId, List<Filter> filters)
        {
            object[] obj = new object[2 + filters.Count];
            obj[0] = requestMessageType.ToString().ToUpper();
            obj[1] = subscriptionId;
            for (var i = 2; i < 2 + filters.Count; i++)
            {
                obj[i] = filters[i - 2];
            }

            string newsub = JsonConvert.SerializeObject(obj);

            var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(newsub));

            await _webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }

        private async Task MessageLoop()
        {
            var messages = new List<string>();
            Console.WriteLine($"setting up listener for {_uri}");
            await foreach (var message in ReceiveLoop())
            {
                NewMessageReceived.Invoke(this, new MessageReceivedEventArgs(_uri, message));
            }
        }

        private async IAsyncEnumerable<NMessage> ReceiveLoop()
        {
            var canceled = false;
            var buffer = new ArraySegment<byte>(new byte[2048]);
            while (true)
            {
                WebSocketReceiveResult result;
                using var ms = new MemoryStream();
                try
                {
                    do
                    {
                        result = await _webSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                        ms.Write(buffer.Array!, buffer.Offset, result.Count);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            canceled = true;
                        }
                    } while (!result.EndOfMessage);
                }
                catch (OperationCanceledException ex)
                {
                    canceled = true;
                }

                ms.Seek(0, SeekOrigin.Begin);
                var rawMessage = Encoding.UTF8.GetString(ms.ToArray());
                if (rawMessage == null || rawMessage == "")
                {
                    continue;
                }

                var message = JsonConvert.DeserializeObject<NMessage>(rawMessage);
                Console.WriteLine($"Message received from {_uri}: {message.SubscriptionId}");
                yield return message;
                if (canceled)
                {
                    Console.WriteLine($"Cancelled, unsubscribing from {_uri}");
                    await UnSubscribeAsync(message.SubscriptionId);
                    break;
                }
            }
        }

        public async Task SendNEvent(string serialisedNMessage, string subscriptionId)
        {
            if (IsOpen)
            {
                Console.WriteLine($"Sending NEvent to {_uri} using {subscriptionId}");

                var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(serialisedNMessage));

                await _webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            }
        }

        private async Task UnSubscribeAsync(string subscriptionId)
        {
            if (IsOpen)
            {
                Console.WriteLine($"Unsubscribing from {_uri}, subscriptionid {subscriptionId}");
                object[] obj = { "CLOSE", subscriptionId };

                string closeMessage = JsonConvert.SerializeObject(obj);

                var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(closeMessage));

                await _webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                ActiveSubscriptions.Remove(subscriptionId);
            }
        }

        public async Task Close()
        {
            var unsubscribeTasks = new List<Task>();

            foreach (var activeSubscription in ActiveSubscriptions)
            {
                Task connectionTask = Task.Run(async () => { await UnSubscribeAsync(activeSubscription.Key); });
                unsubscribeTasks.Add(connectionTask);
            }

            await Task.WhenAll(unsubscribeTasks);

            _cancellationTokenSource.Cancel();
            _webSocket.Dispose();
        }
    }
}