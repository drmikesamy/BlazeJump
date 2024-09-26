using NSubstitute;
using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.UserProfile;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.Connections.Events;

namespace BlazeJump.Common.Tests.ServiceTests
{
    [TestFixture]
    public class MessageServiceTests
    {
        private IRelayManager _relayManager;
        private ICryptoService _cryptoService;
        private IUserProfileService _userProfileService;
        private MessageService _sut;

        [SetUp]
        public void SetUp()
        {
            _relayManager = Substitute.For<IRelayManager>();
            _cryptoService = Substitute.For<ICryptoService>();
            _userProfileService = Substitute.For<IUserProfileService>();
            _sut = new MessageService(_relayManager, _cryptoService, _userProfileService);
        }

        [Test]
        public void RelayMessageReceived_ShouldInvokeNewMessageReceivedEvent()
        {
            // Arrange
            var eventArgs = new MessageReceivedEventArgs("wss://test.ws", new NMessage { SubscriptionId = "TestSubscriptionId" });
            MessageReceivedEventArgs? actualMessageEventArgs = null;
            _sut.MessageReceived += (sender, args) => actualMessageEventArgs = args;

            // Act
            _sut.RelayMessageReceived(this, eventArgs);

            // Assert
            Assert.That(actualMessageEventArgs!.Message.SubscriptionId, Is.EqualTo("TestSubscriptionId"));
            Assert.That(actualMessageEventArgs!.Url, Is.EqualTo("wss://test.ws"));
        }

        [Test]
        public async Task FetchNEventsByFilter_ShouldCallQueryRelays()
        {
            // Arrange
            var filters = new List<Filter>();
            var subscriptionId = "test_subscription";
            var requestMessageType = MessageTypeEnum.Req;

            // Act
            await _sut.Fetch(requestMessageType, filters, subscriptionId);

            // Assert
            await _relayManager.Received(1).QueryRelays(Arg.Any<List<string>>(), subscriptionId, requestMessageType, filters);
        }

        [Test]
        public void VerifyNEvent_ShouldCallCryptoServiceWithCorrectParameters()
        {
            // Arrange
            var nEvent = new NEvent { Sig = "signature", Pubkey = "pubkey" };
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
            _relayManager.OpenRelays.Returns(new List<string> { "wss://test.ws" });
            _relayManager.SendNEvent(Arg.Any<NEvent>(), Arg.Any<List<string>>(), Arg.Any<string>()).Returns(Task.FromResult(1));

            NEvent? actualNEvent = null;
            List<string>? actualUris = null;
            string? actualSubscriptionHash = null;

            await _relayManager.SendNEvent(Arg.Do<NEvent>(n => actualNEvent = n), Arg.Do<List<string>>(u => actualUris = u), Arg.Do<string>(s => actualSubscriptionHash = s));
            _userProfileService.NPubKey.Returns("pubkey");

            // Act
            await _sut.Send(kind, message);

            // Assert
            Assert.That(actualNEvent!.Content, Is.EqualTo(message));
            Assert.That(actualUris!.First(), Is.EqualTo("wss://test.ws"));
        }
    }
}
