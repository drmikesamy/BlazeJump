using BlazeJump.Common.Tests.Infrastructure;
using Microsoft.Playwright;

namespace BlazeJump.Common.Tests.Crypto;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class JSCryptoTests : BlazorTest
{
	//private CryptoService _cryptoService { get; set; }
	//private CryptoService _cryptoService2 { get; set; }
	//[SetUp]
	//public void Init()
	//{
	//	_cryptoService = new CryptoService();
	//	_cryptoService2 = new CryptoService();
	//}

	[Test]
	public async Task ShouldGenerateQRCodeDataForSignerAuthentication()
	{
		await Page.GotoAsync(RootUri.AbsoluteUri);

		await Page.GetByRole(AriaRole.Link, new() { Name = "Counter" }).ClickAsync();

		await Page.GetByRole(AriaRole.Button, new() { Name = "Click me" }).ClickAsync();

		await Expect(Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Current count: 1");

		//await Page.GotoAsync("https://localhost:7018");
		//// Arrange
		//var plainText = "This data is not a multiple of 16 bytes length";
		////Generate my keypair
		//_cryptoService.CreateEtherealKeyPair();
		//var myPublicKey = Convert.ToHexString(_cryptoService.EtherealPublicKey.ToBytes());

		////Generate their keypair
		//_cryptoService2.CreateEtherealKeyPair();
		//var theirPublicKey = Convert.ToHexString(_cryptoService2.EtherealPublicKey.ToBytes());

		//// Act
		//var href = await Page.EvaluateAsync<string>("document.location.href");
		//var encryptedText = await _cryptoService.AesEncrypt(plainText, theirPublicKey);
		//var decryptedText = await _cryptoService2.AesDecrypt(encryptedText.Item1, myPublicKey, encryptedText.Item2);

		// Assert
		//Assert.NotNull(encryptedText);
		//Assert.NotNull(decryptedText);
		//Assert.That(plainText, Is.Not.EqualTo(encryptedText.Item1));
		//Assert.That(plainText, Is.EqualTo(decryptedText));
	}
}