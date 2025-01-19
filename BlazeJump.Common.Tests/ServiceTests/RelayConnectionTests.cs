using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using Newtonsoft.Json;
using System.Net.WebSockets;
using BlazeJump.Common.Services.Connections.Factories;
using NSubstitute;

namespace BlazeJump.Common.Tests.ServiceTests
{
    [TestFixture]
    public class RelayConnectionTests
    {
        private RelayConnection _relayConnection;
        private IClientWebSocketWrapper _clientWebSocketWrapper;
        private IClientWebSocketFactory _clientWebSocketFactory;

        [SetUp]
        public void SetUp()
        {
            string uri = "wss://testrelay.com";
            _clientWebSocketWrapper = Substitute.For<IClientWebSocketWrapper>();
            _clientWebSocketWrapper.ConnectAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            _clientWebSocketWrapper.SendAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<WebSocketMessageType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            _clientWebSocketFactory = Substitute.For<IClientWebSocketFactory>();
            _clientWebSocketFactory.Create().Returns(_clientWebSocketWrapper);
            _relayConnection = new RelayConnection(_clientWebSocketFactory, uri);
        }

        [Test]
        public async Task Init_WebSocketStateIsOpen_ShouldDoNothing()
        {
            // Arrange
            _clientWebSocketWrapper.State.Returns(WebSocketState.Open);
            
            // Act
            await _relayConnection.Init();

            // Assert
            _clientWebSocketWrapper.Received(0).Dispose();
            await _clientWebSocketWrapper.Received(0).ConnectAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>());
        }
        
        [Test]
        public async Task Init_WebSocketStateIsAborted_ShouldDisposeAndGetNewWebsocket()
        {
            // Arrange
            _clientWebSocketWrapper.State.Returns(WebSocketState.Aborted);
            
            // Act
            await _relayConnection.Init();

            // Assert
            _clientWebSocketWrapper.Received(1).Dispose();
            await _clientWebSocketWrapper.Received(1).ConnectAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SubscribeAsync_ShouldSendSubscriptionRequest()
        {
            // Arrange
            string subscriptionId = "sub-1";
            var filters = new List<Filter>();
            _clientWebSocketWrapper.State.Returns(WebSocketState.Open);

            // Act
            await _relayConnection.SubscribeAsync(MessageTypeEnum.Req, subscriptionId, filters);

            // Assert
            Assert.That(_relayConnection.ActiveSubscriptions.ContainsKey(subscriptionId), Is.True);
            await _clientWebSocketWrapper.Received(1).SendAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<WebSocketMessageType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }
        
        [Test]
        public async Task SubscribeAsync_AlreadySubscribed_ShouldNotSendSubscriptionRequest()
        {
            // Arrange
            string subscriptionId = "sub-1";
            _relayConnection.ActiveSubscriptions.Add(subscriptionId, true);
            var filters = new List<Filter>();

            // Act
            await _relayConnection.SubscribeAsync(MessageTypeEnum.Req, subscriptionId, filters);

            // Assert
            await _clientWebSocketWrapper.Received(0).SendAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<WebSocketMessageType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }
        
        [Test]
        public async Task SubscribeAsync_WebsocketClosed_ShouldNotSendSubscriptionRequest()
        {
            // Arrange
            _clientWebSocketWrapper.State.Returns(WebSocketState.Closed);
            string subscriptionId = "sub-1";
            var filters = new List<Filter>();

            // Act
            await _relayConnection.SubscribeAsync(MessageTypeEnum.Req, subscriptionId, filters);

			// Assert
			Assert.That(_relayConnection.ActiveSubscriptions.ContainsKey(subscriptionId), Is.False);
            await _clientWebSocketWrapper.Received(0).SendAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<WebSocketMessageType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SendNEvent_ShouldSendSerializedMessage()
        {
            // Arrange
            string subscriptionId = "sub-1";
            var nEvent = new NEvent();
            string serializedMessage = JsonConvert.SerializeObject(new object[] { "EVENT", nEvent });
            _clientWebSocketWrapper.State.Returns(WebSocketState.Open);

            // Act
            await _relayConnection.Init();
            await _relayConnection.SendNEvent(serializedMessage, subscriptionId);

            // Assert
            await _clientWebSocketWrapper.Received(1).SendAsync(Arg.Any<ArraySegment<byte>>(),
                Arg.Any<WebSocketMessageType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }
        
        [Test]
        public async Task SendNEvent_WebsocketClosed_ShouldNotSendSerializedMessage()
        {
            // Arrange
            _clientWebSocketWrapper.State.Returns(WebSocketState.Closed);
            string subscriptionId = "sub-1";
            var nEvent = new NEvent();
            string serializedMessage = JsonConvert.SerializeObject(new object[] { "EVENT", nEvent });

            // Act
            await _relayConnection.SendNEvent(serializedMessage, subscriptionId);

            // Assert
            await _clientWebSocketWrapper.Received(0).SendAsync(Arg.Any<ArraySegment<byte>>(),
                Arg.Any<WebSocketMessageType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Close_ShouldUnsubscribeAndCloseWebSocket()
        {
            // Arrange
            string subscriptionId = "sub-1";
            _relayConnection.ActiveSubscriptions.Add(subscriptionId, true);
            var filters = new List<Filter>();
            _clientWebSocketWrapper.State.Returns(WebSocketState.Open);

            // Act
            await _relayConnection.Init();
            await _relayConnection.Close();

            // Assert
            await _clientWebSocketWrapper.Received(1).SendAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<WebSocketMessageType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
            Assert.That(_relayConnection.ActiveSubscriptions.ContainsKey(subscriptionId), Is.False);
            _clientWebSocketWrapper.Received(1).Dispose();
        }
    }
}
