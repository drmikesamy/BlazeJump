using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Identity;
using BlazeJump.Common.Services.Message;
using BlazeJump.Native.Services.Crypto;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazeJump.Native.Tests.ServiceTests.Identity
{
	public  class NativeIdentityServiceTests
	{
		private IdentityService _identityService;
		private Mock<IRelayManager> _relayManagerMock;
		private Mock<IMessageService> _messageServiceMock;

		[SetUp]
		public void SetUp()
		{
			_relayManagerMock = new Mock<IRelayManager>();
			_messageServiceMock = new Mock<IMessageService>();
			_identityService = new IdentityService(_relayManagerMock.Object, _messageServiceMock.Object);
		}

		[Test]
		public void OnQrConnectReceived_ShouldSendNostrConnectReply()
		{
			// Arrange
			var pubkey = "testPubkey";
			var secret = "testSecret";
			var eventArgs = new QrConnectEventArgs { Pubkey = pubkey, Secret = secret };

			_messageServiceMock
				.Setup(m => m.CreateNEvent(It.IsAny<KindEnum>(), It.IsAny<string>(), null, null, It.IsAny<List<string>>()))
				.Returns(new NEvent());

			// Act
			_identityService.OnQrConnectReceived(eventArgs);

			// Assert
			_messageServiceMock.Verify(m => m.CreateNEvent(KindEnum.NostrConnect, It.IsAny<string>(), null, null, It.Is<List<string>>(list => list.Contains(pubkey))), Times.Once);
			_messageServiceMock.Verify(m => m.Send(KindEnum.NostrConnect, It.IsAny<NEvent>(), pubkey), Times.Once);
		}
	}
}
