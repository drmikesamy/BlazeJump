/**
 * @jest-environment jsdom
 */

const sum = require('../BlazeJump.Common/wwwroot/scripts/subtlecrypto');

test('Can perform AES-CBC encrypt and decrypt', () => {
    var plainText = "This data is not a multiple of 16 bytes length";
    var pubKey1 = '02805E5C52600A2798354736A9982E20B40FEB087AC6B257F5AE95B9B5BEAD5DB3';
    var privKey1 = 
    var pubKey2 = 
    var privKey2 = 
    expect(sum(1, 2)).toBe(3);
});

//    public async Task ShouldGenerateQRCodeDataForSignerAuthentication()
//    {
//		await Page.GotoAsync("https://localhost:7018/login");
//		// Arrange
//		var plainText = "This data is not a multiple of 16 bytes length";
//		//Generate my keypair
//		_cryptoService.CreateEtherealKeyPair();
//		var myPublicKey = Convert.ToHexString(_cryptoService.EtherealPublicKey.ToBytes());

//		//Generate their keypair
//		_cryptoService2.CreateEtherealKeyPair();
//		var theirPublicKey = Convert.ToHexString(_cryptoService2.EtherealPublicKey.ToBytes());

//		// Act
//		var encryptedText = await _cryptoService.AesEncrypt(plainText, theirPublicKey);
//		//var decryptedText = await _cryptoService2.AesDecrypt(encryptedText.Item1, myPublicKey, encryptedText.Item2);

//		// Assert
//		//Assert.NotNull(encryptedText);
//		//Assert.NotNull(decryptedText);
//		//Assert.That(plainText, Is.Not.EqualTo(encryptedText.Item1));
//		//Assert.That(plainText, Is.EqualTo(decryptedText));
//	}