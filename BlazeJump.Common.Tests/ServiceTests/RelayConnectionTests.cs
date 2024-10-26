using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace BlazeJump.Common.Tests.ServiceTests
{
    [TestFixture]
    public class RelayConnectionTests
    {
        private RelayConnection _relayConnection;

        [SetUp]
        public void SetUp()
        {
            string uri = "wss://testrelay.com";
            _relayConnection = new RelayConnection(uri);
        }

        [Test]
        public async Task Init_ShouldConnectAndStartMessageLoop()
        {
            // Act
            await _relayConnection.Init();

            // Assert
            Assert.AreEqual(WebSocketState.Open, _relayConnection.WebSocket.State);
        }

        [Test]
        public async Task SubscribeAsync_ShouldSendSubscriptionRequest()
        {
            // Arrange
            string subscriptionId = "sub-1";
            var filters = new List<Filter>();

            // Act
            await _relayConnection.SubscribeAsync(MessageTypeEnum.Req, subscriptionId, filters);

            // Assert
            Assert.IsTrue(_relayConnection.ActiveSubscriptions.ContainsKey(subscriptionId));
        }

        [Test]
        public async Task SendNEvent_ShouldSendSerializedMessage()
        {
            // Arrange
            string subscriptionId = "sub-1";
            var nEvent = new NEvent();
            string serializedMessage = JsonConvert.SerializeObject(new object[] { "EVENT", nEvent });

            // Act
            await _relayConnection.SendNEvent(serializedMessage, subscriptionId);

            // Assert
            var buffer = new byte[4096];
            var result = await _relayConnection.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Assert.AreEqual(serializedMessage, receivedMessage);
        }

        [Test]
        public async Task Close_ShouldUnsubscribeAndCloseWebSocket()
        {
            // Act
            await _relayConnection.Close();

            // Assert
            Assert.AreEqual(WebSocketState.Closed, _relayConnection.WebSocket.State);
        }
    }
}
