using BlazeJump.Common.Services.Crypto;
using Microsoft.JSInterop;
using NSubstitute;

namespace BlazeJump.Tests.Services.Crypto
{
	[TestFixture]
	public class CryptoServiceTests
	{
		private IBrowserCrypto _browserCrypto;
		private CryptoService _cryptoService;

		[SetUp]
		public void SetUp()
		{
			_browserCrypto = Substitute.For<IBrowserCrypto>();
			_cryptoService = new CryptoService(_browserCrypto);
		}

		[Test]
		public void CreateEtherealKeyPair_ShouldGenerateKeyPair()
		{
			// Act
			_cryptoService.CreateEtherealKeyPair();

			// Assert
			Assert.IsNotNull(_cryptoService.EtherealPublicKey);
		}

		[Test]
		public void GetNewSecp256k1KeyPair_ShouldReturnKeyPair()
		{
			// Act
			var keyPair = _cryptoService.GetNewSecp256k1KeyPair();

			// Assert
			Assert.IsNotNull(keyPair);
			Assert.IsNotNull(keyPair.PrivateKey);
			Assert.IsNotNull(keyPair.PublicKey);
		}

		[Test]
		public async Task AesEncrypt_ShouldReturnEncryptedTextAndIv()
		{
			// Arrange
			var plainText = "Hello World";
			_cryptoService.CreateEtherealKeyPair();
			var theirPublicKey = Convert.ToHexString(_cryptoService.EtherealPublicKey.ToBytes());
			_browserCrypto.InvokeBrowserCrypto("aesEncrypt", Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>())
				.Returns("encryptedText");

			// Act
			var result = await _cryptoService.AesEncrypt(plainText, theirPublicKey);

			// Assert
			Assert.IsNotNull(result);
			Assert.That(result.Item1, Is.EqualTo("encryptedText"));
			Assert.IsNotNull(result.Item2);
		}

		[Test]
		public async Task AesDecrypt_ShouldReturnDecryptedText()
		{
			// Arrange
			var base64CipherText = "encryptedText";
			_cryptoService.CreateEtherealKeyPair();
			var theirPublicKey = Convert.ToHexString(_cryptoService.EtherealPublicKey.ToBytes());
			var ivString = "ivString";
			var sharedPoint = new byte[32];
			_browserCrypto.InvokeBrowserCrypto("aesDecrypt", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns("decryptedText");

			// Act
			var result = await _cryptoService.AesDecrypt(base64CipherText, theirPublicKey, ivString);

			// Assert
			Assert.That(result, Is.EqualTo("decryptedText"));
		}

		[Test]
		public void Sign_ShouldReturnSignature()
		{
			// Arrange
			_cryptoService.CreateEtherealKeyPair();
			var message = "Hello World";

			// Act
			var signature = _cryptoService.Sign(message);

			// Assert
			Assert.IsNotNull(signature);
			Assert.That(signature.Length, Is.EqualTo(128));
		}

		[Test]
		public void Verify_ShouldReturnTrueForValidSignature()
		{
			// Arrange
			_cryptoService.CreateEtherealKeyPair();
			var message = "Hello World";
			var signature = _cryptoService.Sign(message);
			var xOnlyPublicKey = Convert.ToHexString(_cryptoService.EtherealPublicKey.ToBytes()[1..]);

			// Act
			var isValid = _cryptoService.Verify(signature, message, xOnlyPublicKey);

			// Assert
			Assert.IsTrue(isValid);
		}
	}
}
