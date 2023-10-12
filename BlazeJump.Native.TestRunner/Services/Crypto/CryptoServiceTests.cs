using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Crypto;
using Microsoft.JSInterop;

namespace BlazeJump.Native.TestRunner.Services.Crypto
{
	public class CryptoServiceTests
	{
		private CryptoService _cryptoService { get; set; }
		public CryptoServiceTests(IJSRuntime jsRuntime)
		{
			_cryptoService = new CryptoService(jsRuntime);
		}

		[Fact]
		public async Task GenerateSecp256K1KeyPair_ShouldGenerateValidKeyPair()
		{
			// Arrange
			var expectedPublicKeyLength = 64;
			var expectedPrivateKeyLength = 64;

			// Act
			var keyPair = await _cryptoService.GenerateKeyPair();
			var publicKey = keyPair.PublicKey;
			var privateKey = keyPair.PrivateKey;

			// Assert
			Assert.NotNull(publicKey);
			Assert.NotNull(privateKey);
			Assert.Equal(expectedPublicKeyLength, publicKey.Length);
			Assert.Equal(expectedPrivateKeyLength, privateKey.Length);
		}

		[Fact]
		public async Task Encrypt_ShouldReturnEncryptedText()
		{
			// Arrange
			var plainText = "Hello world";

			var myKeyPair = await _cryptoService.GenerateKeyPair();
			var theirKeyPair = await _cryptoService.GenerateKeyPair();

			var theirPublicKey = theirKeyPair.PublicKey;
			var myPrivateKey = myKeyPair.PrivateKey;

			// Act
			var encryptedText = await _cryptoService.AesEncrypt(plainText, theirPublicKey, myPrivateKey);

			// Assert
			Assert.NotNull(encryptedText);
			Assert.NotEqual(plainText, encryptedText);
		}

		//[Fact]
		//public async Task SignEvent_ShouldReturnSignedEvent()
		//{
		//	// Arrange

		//	var keyPair = await _cryptoService.GenerateKeyPair();

		//	var nEvent = new NEvent
		//	{
		//		Kind = KindEnum.Text,
		//		Content = "Hello world",
		//		Pubkey = keyPair.PublicKey,
		//		Created_At = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()
		//	};
		//	var expectedIdLength = 64;
		//	var expectedSigLength = 128;		

		//	// Act
		//	var signedEvent = await _cryptoService.SignEvent(nEvent);

		//	// Assert
		//	Assert.NotNull(signedEvent);
		//	Assert.Equal(nEvent.Kind, signedEvent.Kind);
		//	Assert.Equal(nEvent.Content, signedEvent.Content);
		//	Assert.Equal(nEvent.Pubkey, signedEvent.Pubkey);
		//	Assert.Equal(nEvent.Created_At, signedEvent.Created_At);
		//	Assert.NotNull(signedEvent.Id);
		//	Assert.NotNull(signedEvent.Sig);
		//	Assert.Equal(expectedIdLength, signedEvent.Id.Length);
		//	Assert.Equal(expectedSigLength, signedEvent.Sig.Length);
		//}
	}
}