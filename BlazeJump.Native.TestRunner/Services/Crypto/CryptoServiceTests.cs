using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Crypto;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace BlazeJump.Native.TestRunner.Services.Crypto
{
	public class CryptoServiceTests
	{
		private ICryptoService _cryptoService { get; set; }
		public CryptoServiceTests(ICryptoService cryptoService)
		{
			_cryptoService = cryptoService;
		}

		[Fact]
		public void GenerateSecp256K1KeyPair_ShouldGenerateValidKeyPair()
		{
			// Arrange
			var expectedPublicKeyLength = 64;

			// Act
			var keyPair = _cryptoService.GenerateKeyPair();

			// Assert
			Assert.NotNull(_cryptoService.PublicKey);
			Assert.Equal(expectedPublicKeyLength, _cryptoService.XOnlyPublicKey.Length);
		}

		[Fact]
		public async Task WebEncryptDecrypt_ShouldEncryptAndDecryptString()
		{
			// Arrange
			var plainText = "This data is not a multiple of 16 bytes length";
			//Generate their keypair
			_cryptoService.GenerateKeyPair();
			var theirPublicKey = _cryptoService.XOnlyPublicKey;

			//Generate my keypair
			_cryptoService.GenerateKeyPair();
			var myPublicKey = _cryptoService.XOnlyPublicKey;

			// Act
			var encryptedText = await _cryptoService.AesEncrypt(plainText, myPublicKey);
			var decryptedText = await _cryptoService.AesDecrypt(encryptedText.Item1, theirPublicKey, encryptedText.Item2);

			// Assert
			Assert.NotNull(encryptedText);
			Assert.NotNull(decryptedText);
			Assert.NotEqual(plainText, encryptedText.Item1);
			Assert.Equal(plainText, decryptedText);
		}
		[Fact]
		public void NativeEncryptDecrypt_ShouldEncryptAndDecryptString()
		{
			// Arrange
			var plainText = "This data is not a multiple of 16 bytes length";
			//Generate their keypair
			_cryptoService.GenerateKeyPair();
			var theirPublicKey = _cryptoService.PublicKey;
			//Generate my keypair
			_cryptoService.GenerateKeyPair();
			var myPublicKey = _cryptoService.PublicKey;

			// Act
			var encryptedText = _cryptoService.NativeAesEncrypt(plainText, myPublicKey);
			var decryptedText = _cryptoService.NativeAesDecrypt(encryptedText.Item1, theirPublicKey, encryptedText.Item2);

			// Assert
			Assert.NotNull(encryptedText);
			Assert.NotNull(decryptedText);
			Assert.NotEqual(plainText, encryptedText.Item1);
			Assert.Equal(plainText, decryptedText);
		}
		[Fact]
		public async Task NativeAndWebCombinedEncryptDecrypt_ResultsShouldBeIdentical()
		{
			// Arrange
			var plainText = "This data is not a multiple of 16 bytes length";
			//Generate their keypair
			_cryptoService.GenerateKeyPair();
			var theirPublicKey = _cryptoService.PublicKey;
			//Generate my keypair
			_cryptoService.GenerateKeyPair();
			var myPublicKey = _cryptoService.PublicKey;

			Random rand = new Random();
			byte[] ivBytes = new byte[16];
			rand.NextBytes(ivBytes);
			var iv = Convert.ToBase64String(ivBytes);

			// Act
			var encryptedText = await _cryptoService.AesEncrypt(plainText, myPublicKey, iv);
			var decryptedText = await _cryptoService.AesDecrypt(encryptedText.Item1, theirPublicKey, encryptedText.Item2);

			var encryptedTextNative = _cryptoService.NativeAesEncrypt(plainText, myPublicKey, iv);
			var decryptedTextNative = _cryptoService.NativeAesDecrypt(encryptedText.Item1, theirPublicKey, encryptedText.Item2);

			// Assert
			Assert.NotNull(encryptedText);
			Assert.NotNull(decryptedText);

			Assert.NotNull(encryptedTextNative);
			Assert.NotNull(decryptedTextNative);

			Assert.NotEqual(plainText, encryptedText.Item1);
			Assert.NotEqual(plainText, encryptedTextNative.Item1);

			Assert.Equal(plainText, decryptedText);
			Assert.Equal(plainText, decryptedTextNative);

			Assert.Equal(encryptedText, encryptedTextNative);
			Assert.Equal(decryptedText, decryptedTextNative);
		}

		[Fact]
		public async Task Sign_ShouldReturnValidSignature()
		{
			// Arrange

			_cryptoService.GenerateKeyPair();

			var testString = "test string";

			var expectedSigLength = 128;

			// Act
			var signature = _cryptoService.Sign(testString);

			// Assert
			Assert.NotNull(signature);
			Assert.Equal(expectedSigLength, signature.Length);

			var validSignature = _cryptoService.Verify(signature, testString, _cryptoService.PublicKey);
		}
	}
}