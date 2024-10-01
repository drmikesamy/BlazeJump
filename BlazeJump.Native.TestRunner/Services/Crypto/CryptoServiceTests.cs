using BlazeJump.Common.Services.Crypto;

namespace BlazeJump.Native.TestRunner.Services.Crypto
{
	public class CryptoServiceTests
	{
		private CryptoService _cryptoService { get; set; }
		private NativeCryptoService _nativeCryptoService { get; set; }
		private NativeCryptoService _nativeCryptoService2 { get; set; }
		public CryptoServiceTests()
		{
			_cryptoService = new CryptoService();
			_nativeCryptoService = new NativeCryptoService();
			_nativeCryptoService2 = new NativeCryptoService();
		}

		[Fact]
		public void GenerateSecp256K1KeyPair_ShouldGenerateValidKeyPair()
		{
			// Arrange
			var expectedPublicKeyLength = 33;

			// Act
			var keyPair = _cryptoService.GetNewSecp256k1KeyPair();

			// Assert
			Assert.NotNull(keyPair.PublicKey);
			Assert.Equal(expectedPublicKeyLength, keyPair.PublicKey.ToBytes().Length);
		}

		//[Fact]
		//public async Task WebEncryptDecrypt_ShouldEncryptAndDecryptString()
		//{
		//	// Arrange
		//	var plainText = "This data is not a multiple of 16 bytes length";
		//	//Generate their keypair
		//	var theirKeyPair = _cryptoService.GetNewSecp256k1KeyPair();
		//	var theirPublicKey = Convert.ToHexString(theirKeyPair.XOnlyPublicKey.ToBytes());

		//	//Generate my keypair
		//	var myKeyPair = _cryptoService.GetNewSecp256k1KeyPair();
		//	var myPublicKey = Convert.ToHexString(myKeyPair.XOnlyPublicKey.ToBytes());

		//	// Act
		//	var encryptedText = await _cryptoService.AesEncrypt(plainText, myPublicKey);
		//	var decryptedText = await _cryptoService.AesDecrypt(encryptedText.Item1, theirPublicKey, encryptedText.Item2);

		//	// Assert
		//	Assert.NotNull(encryptedText);
		//	Assert.NotNull(decryptedText);
		//	Assert.NotEqual(plainText, encryptedText.Item1);
		//	Assert.Equal(plainText, decryptedText);
		//}
		[Fact]
		public async Task NativeEncryptDecrypt_ShouldEncryptAndDecryptString()
		{   
			// Arrange
			var plainText = "This data is not a multiple of 16 bytes length";
			//Generate my keypair
			_nativeCryptoService.CreateEtherealKeyPair();
			var myPublicKey = Convert.ToHexString(_nativeCryptoService.EtherealPublicKey.ToBytes());

			//Generate their keypair
			_nativeCryptoService2.CreateEtherealKeyPair();
			var theirPublicKey = Convert.ToHexString(_nativeCryptoService2.EtherealPublicKey.ToBytes());

			// Act
			var encryptedText = await _nativeCryptoService.AesEncrypt(plainText, theirPublicKey);
			var decryptedText = await _nativeCryptoService2.AesDecrypt(encryptedText.Item1, myPublicKey, encryptedText.Item2);

			// Assert
			Assert.NotNull(encryptedText);
			Assert.NotNull(decryptedText);
			Assert.NotEqual(plainText, encryptedText.Item1);
			Assert.Equal(plainText, decryptedText);
		}
		//[Fact]
		//public async Task NativeAndWebCombinedEncryptDecrypt_ResultsShouldBeIdentical()
		//{
		//	// Arrange
		//	var plainText = "This data is not a multiple of 16 bytes length";
		//	//Generate their keypair
		//	var theirKeyPair = _cryptoService.GetNewSecp256k1KeyPair();
		//	var theirPublicKey = Convert.ToHexString(theirKeyPair.XOnlyPublicKey.ToBytes());

		//	//Generate my keypair
		//	var myKeyPair = _cryptoService.GetNewSecp256k1KeyPair();
		//	var myPublicKey = Convert.ToHexString(myKeyPair.XOnlyPublicKey.ToBytes());

		//	Random rand = new Random();
		//	byte[] ivBytes = new byte[16];
		//	rand.NextBytes(ivBytes);
		//	var iv = Convert.ToBase64String(ivBytes);

		//	// Act
		//	var encryptedText = await _cryptoService.AesEncrypt(plainText, myPublicKey, iv);
		//	var decryptedText = await _cryptoService.AesDecrypt(encryptedText.Item1, theirPublicKey, encryptedText.Item2);

		//	var encryptedTextNative = await _nativeCryptoService.AesEncrypt(plainText, myPublicKey, iv);
		//	var decryptedTextNative = await _nativeCryptoService.AesDecrypt(encryptedText.Item1, theirPublicKey, encryptedText.Item2);

		//	// Assert
		//	Assert.NotNull(encryptedText);
		//	Assert.NotNull(decryptedText);

		//	Assert.NotNull(encryptedTextNative);
		//	Assert.NotNull(decryptedTextNative);

		//	Assert.NotEqual(plainText, encryptedText.Item1);
		//	Assert.NotEqual(plainText, encryptedTextNative.Item1);

		//	Assert.Equal(plainText, decryptedText);
		//	Assert.Equal(plainText, decryptedTextNative);

		//	Assert.Equal(encryptedText, encryptedTextNative);
		//	Assert.Equal(decryptedText, decryptedTextNative);
		//}

		[Fact]
		public void Sign_ShouldReturnValidSignature()
		{
			// Arrange

			//Generate ethereal keypair
			_cryptoService.CreateEtherealKeyPair();

			var testString = "test string";

			var expectedSigLength = 128;

			// Act
			var signature = _cryptoService.Sign(testString);
			var validSignature = _cryptoService.Verify(signature, testString, Convert.ToHexString(_cryptoService.EtherealPublicKey.ToBytes()[1..]));

			// Assert
			Assert.NotNull(signature);
			Assert.Equal(expectedSigLength, signature.Length);
			Assert.True(validSignature);
		}
	}
}