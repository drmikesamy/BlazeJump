using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Crypto;
using Microsoft.JSInterop;

namespace BlazeJump.Native.TestRunner.Services.Crypto
{
	public class CryptoServiceTests
	{
		private CryptoService _cryptoService { get; set; }
		public CryptoServiceTests()
		{
			_cryptoService = new CryptoService();
		}

		[Fact]
		public void GenerateSecp256K1KeyPair_ShouldGenerateValidKeyPair()
		{
			// Arrange
			var expectedPublicKeyLength = 64;
			var expectedPrivateKeyLength = 64;

			// Act
			var keyPair = _cryptoService.GenerateKeyPair();
			var publicKey = keyPair.PublicKey;
			var privateKey = keyPair.PrivateKey;

			// Assert
			Assert.NotNull(publicKey);
			Assert.NotNull(privateKey);
			Assert.Equal(expectedPublicKeyLength, publicKey.Length);
			Assert.Equal(expectedPrivateKeyLength, privateKey.Length);
		}

		[Fact]
		public void WebEncryptDecrypt_ShouldEncryptAndDecryptString()
		{
			// Arrange
			var plainText = "This data is not a multiple of 16 bytes length";

			var myKeyPair = _cryptoService.GenerateKeyPair();
			var myPublicKey = myKeyPair.PublicKey;
			var myPrivateKey = myKeyPair.PrivateKey;

			var theirKeyPair = _cryptoService.GenerateKeyPair();
			var theirPublicKey = theirKeyPair.PublicKey;
			var theirPrivateKey = theirKeyPair.PrivateKey;

			// Act
			var encryptedText = _cryptoService.AesEncrypt(plainText, theirPublicKey, myPrivateKey);
			var decryptedText = _cryptoService.AesDecrypt(encryptedText.Item1, myPublicKey, theirPrivateKey, encryptedText.Item2);

			// Assert
			Assert.NotNull(encryptedText);
			Assert.NotNull(decryptedText);
			Assert.NotEqual(plainText, encryptedText.Item1);
			Assert.Equal(plainText, decryptedText);
		}
#if ANDROID
		[Fact]
		public void NativeEncryptDecrypt_ShouldEncryptAndDecryptString()
		{
			// Arrange
			var plainText = "This data is not a multiple of 16 bytes length";

			var myKeyPair = _cryptoService.GenerateKeyPair();
			var myPublicKey = myKeyPair.PublicKey;
			var myPrivateKey = myKeyPair.PrivateKey;

			var theirKeyPair = _cryptoService.GenerateKeyPair();
			var theirPublicKey = theirKeyPair.PublicKey;
			var theirPrivateKey = theirKeyPair.PrivateKey;

			// Act
			var encryptedText = _cryptoService.Encrypt(plainText, theirPublicKey, myPrivateKey);
			var decryptedText = _cryptoService.Decrypt(encryptedText.Item1, myPublicKey, theirPrivateKey, encryptedText.Item2);

			// Assert
			Assert.NotNull(encryptedText);
			Assert.NotNull(decryptedText);
			Assert.NotEqual(plainText, encryptedText.Item1);
			Assert.Equal(plainText, decryptedText);
		}
#endif

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