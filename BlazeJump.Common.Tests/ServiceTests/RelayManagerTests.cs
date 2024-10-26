using System.Net.WebSockets;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Connections.Events;
using BlazeJump.Common.Enums;
using NSubstitute;

namespace BlazeJump.Common.Tests.ServiceTests
{
    [TestFixture]
    public class RelayManagerTests
    {
        private RelayManager _relayManager;
        private IRelayConnection _relayConnection;
        private IRelayConnectionProvider _relayConnectionProvider;

        [SetUp]
        public void SetUp()
        {
            _relayConnectionProvider = Substitute.For<IRelayConnectionProvider>();
            _relayConnection = Substitute.For<IRelayConnection>();
            _relayConnection.WebSocket.Returns(new ClientWebSocket());
            _relayManager = new RelayManager(_relayConnectionProvider);
        }

        [Test]
        public async Task OpenConnection_ShouldAddConnectionToDictionaryAndInit()
        {
            // Arrange
            string uri = "wss://newrelay.com";
            _relayConnectionProvider.CreateRelayConnection(uri).Returns(_relayConnection);

            // Act
            await _relayManager.OpenConnection(uri);

            // Assert
            Assert.IsTrue(_relayManager.RelayConnections.ContainsKey(uri));
            await _relayConnection.Received(1).Init();
        }

        [Test]
        public async Task CloseConnection_ShouldCloseExistingRelayConnection()
        {
            // Arrange
            string uri = "wss://newrelay.com";
            _relayManager.RelayConnections.Add(uri, _relayConnection);

            // Act
            await _relayManager.CloseConnection(uri);

            // Assert
            await _relayConnection.Received(1).Close();
        }

        [Test]
        public async Task QueryRelays_ShouldSubscribeAllRelays()
        {
            // Arrange
            string uri = "wss://newrelay.com";
            string uri2 = "wss://newrelay2.com";
            _relayConnectionProvider.CreateRelayConnection(uri).Returns(_relayConnection);
            _relayConnectionProvider.CreateRelayConnection(uri2).Returns(_relayConnection);
            _relayManager.RelayConnections = new Dictionary<string, IRelayConnection>
            {
                {uri, _relayConnection},
                {uri2, _relayConnection}
            };
            string subscriptionId = "sub1";
            MessageTypeEnum requestMessageType = MessageTypeEnum.Req;
            var filters = new List<Filter>();

            // Act
            await _relayManager.QueryRelays(subscriptionId, requestMessageType, filters);

            // Assert
            await _relayConnection.Received(2).SubscribeAsync(requestMessageType, subscriptionId, filters);
        }

        [Test]
        public async Task AddToQueue_ShouldEnqueueReceivedMessage()
        {
            // Arrange
            string uri = "wss://newrelay.com";
            var messageEventArgs = new MessageReceivedEventArgs("uri", new NMessage());
            _relayConnectionProvider.CreateRelayConnection(uri).Returns(_relayConnection);

            // Act
            await _relayManager.OpenConnection(uri);
            _relayConnection.NewMessageReceived +=
                Raise.EventWith<MessageReceivedEventArgs>(new object(), messageEventArgs);

            // Assert
            Assert.That(_relayManager.ReceivedMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task SendNEvent_ShouldSendEvent()
        {
            // Arrange
            string uri = "wss://newrelay.com";
            string uri2 = "wss://newrelay2.com";
            _relayConnectionProvider.CreateRelayConnection(uri).Returns(_relayConnection);
            _relayConnectionProvider.CreateRelayConnection(uri2).Returns(_relayConnection);
            _relayManager.RelayConnections = new Dictionary<string, IRelayConnection>
            {
                {uri, _relayConnection},
                {uri2, _relayConnection}
            };
            string subscriptionId = "sub1";
            NEvent nEvent = new NEvent();

            // Act
            await _relayManager.SendNEvent(nEvent, subscriptionId);

            // Assert
            await _relayConnection.Received(2).SendNEvent(Arg.Any<string>(), subscriptionId);
        }
    }
}