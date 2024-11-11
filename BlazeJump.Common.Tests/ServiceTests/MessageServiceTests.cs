using System.Diagnostics;
using NSubstitute;
using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.UserProfile;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.Notification;

namespace BlazeJump.Common.Tests.ServiceTests
{
    [TestFixture]
    public class MessageServiceTests
    {
        private IRelayManager _relayManager;
        private ICryptoService _cryptoService;
        private IUserProfileService _userProfileService;
        private INotificationService _notificationService;
        private MessageService _sut;

        [SetUp]
        public void SetUp()
        {
            _relayManager = Substitute.For<IRelayManager>();
            _cryptoService = Substitute.For<ICryptoService>();
            _userProfileService = Substitute.For<IUserProfileService>();
            _notificationService = Substitute.For<INotificationService>();
            _sut = new MessageService(_relayManager, _cryptoService, _userProfileService, _notificationService);
        }

        [Test]
        public void ProcessMessageQueueEvent_ProcessesQueuedMessages()
        {
            // Arrange
            var queue = new PriorityQueue<NMessage, Tuple<int, long>>();
            queue.Enqueue(
                new NMessage
                {
                    SubscriptionId = "second", MessageType = MessageTypeEnum.Event,
                    Event = new NEvent
                    {
                        Kind = KindEnum.Metadata, 
                        Id = "secondEventId", 
                        UserId = "secondUserId",
                    }
                }, new Tuple<int, long>(1, Stopwatch.GetTimestamp()));
            
            queue.Enqueue(
                new NMessage
                {
                    SubscriptionId = "first", MessageType = MessageTypeEnum.Event,
                    Event = new NEvent
                    {
                        Kind = KindEnum.Text, 
                        Id = "firstEventId", 
                        UserId = "firstUserId",
                        Tags = new List<EventTag>
                        {
                            new EventTag
                            {
                                Key = TagEnum.e,
                                Value = "taggedParentEventId",
                                Value3 = "reply"
                            },
                            new EventTag
                            {
                                Key = TagEnum.e,
                                Value = "taggedRootEventId",
                                Value3 = "root"
                            },
                        }
                    }
                }, new Tuple<int, long>(0, Stopwatch.GetTimestamp()));
            
            queue.Enqueue(
                new NMessage { SubscriptionId = "third", MessageType = MessageTypeEnum.Eose },
                new Tuple<int, long>(2, Stopwatch.GetTimestamp()));
            
            _relayManager.ReceivedMessages.Returns(x => queue);

            // Act
            _relayManager.ProcessMessageQueue += Raise.Event<EventHandler>(new object(), null);

            // Assert
            _sut.RelationRegister.TryGetRelations(new List<string>() { "firstEventId" }, RelationTypeEnum.TaggedRootId, out var firstEventRootId);
            _sut.RelationRegister.TryGetRelations(new List<string>() { "firstEventId" }, RelationTypeEnum.TaggedParentIds, out var firstEventParentId);
            
            Assert.That(_sut.MessageStore.Count(), Is.EqualTo(2));
            Assert.That(firstEventRootId.First(), Is.EqualTo("taggedRootEventId"));
            Assert.That(firstEventParentId.First(), Is.EqualTo("taggedParentEventId"));
        }

        [Test]
        public async Task FetchNEventsByFilter_ShouldCallQueryRelays()
        {
            // Arrange
            var filters = new List<Filter>();
            var subscriptionId = "test_subscription";
            var requestMessageType = MessageTypeEnum.Req;

            // Act
            await _sut.Fetch(filters, subscriptionId, requestMessageType);

            // Assert
            await _relayManager.Received(1).QueryRelays(subscriptionId, requestMessageType, filters);
        }

        [Test]
        public void VerifyNEvent_ShouldCallCryptoServiceWithCorrectParameters()
        {
            // Arrange
            var nEvent = new NEvent { Sig = "signature", UserId = "pubkey" };
            _cryptoService.Verify("signature", Arg.Any<string>(), "pubkey").Returns(true);

            // Act
            var result = _sut.Verify(nEvent);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task SendNEvent_ShouldCallRelayManagerSendWithCorrectParams()
        {
            // Arrange
            var kind = KindEnum.Text;
            var message = "test message";
            _cryptoService.Sign(Arg.Any<string>()).Returns("signature");
            _relayManager.Relays.Returns(new List<string> { "wss://test.ws" });
            _relayManager.SendNEvent(Arg.Any<NEvent>(), Arg.Any<string>())
                .Returns(Task.FromResult(1));

            NEvent? actualNEvent = null;
            string? actualSubscriptionHash = null;

            await _relayManager.SendNEvent(Arg.Do<NEvent>(n => actualNEvent = n),
                Arg.Do<string>(s => actualSubscriptionHash = s));
            _userProfileService.NPubKey.Returns("pubkey");

            // Act
            await _sut.Send(kind, message);

            // Assert
            Assert.That(actualNEvent!.Content, Is.EqualTo(message));
        }
    }
}