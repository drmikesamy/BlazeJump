using BlazeJump.Native.Services.Crypto;
using Moq;

namespace BlazeJump.Native.Tests.ServiceTests.Crypto
{
	[TestFixture]
	public class NativeCryptoServiceTests
	{
		private BaseCryptoService _cryptoService;
		private BaseCryptoService _cryptoService2;

		[SetUp]
		public void SetUp()
		{
			_cryptoService = new BaseCryptoService();
			_cryptoService2 = new BaseCryptoService();
		}

		[Test]
		public async Task AesEncryptDecrypt_ShouldEncryptAndDecryptString()
		{
			// Arrange
			var plainText = "This data is not a multiple of 16 bytes length";
			//Generate my keypair
			_cryptoService.CreateEtherealKeyPair();
			var myPublicKey = Convert.ToHexString(_cryptoService.EtherealPublicKey.ToBytes());

			//Generate their keypair
			_cryptoService2.CreateEtherealKeyPair();
			var theirPublicKey = Convert.ToHexString(_cryptoService2.EtherealPublicKey.ToBytes());

			// Act
			var encryptedText = await _cryptoService.AesEncrypt(plainText, theirPublicKey);
			var decryptedText = await _cryptoService2.AesDecrypt(encryptedText.CipherText, myPublicKey, encryptedText.Iv);

			// Assert
			Assert.That(encryptedText, Is.Not.Null);
			Assert.That(decryptedText, Is.Not.Null);
			Assert.That(plainText, Is.Not.EqualTo(encryptedText.CipherText));
			Assert.That(plainText, Is.EqualTo(decryptedText));
		}
	}
}
