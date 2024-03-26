using BlazeJump.Common.Models.Crypto;
using BlazeJump.Helpers;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Microsoft.Maui.Platform;
using NBitcoin.Secp256k1;
using System.Text;

namespace BlazeJump.Common.Services.Crypto
{
	public partial class CryptoService : ICryptoService
	{
		public ECXOnlyPubKey EtherealPublicKey => _etherealKeyPair.XOnlyPublicKey;
		private Secp256k1KeyPair _etherealKeyPair { get; set; }

		private readonly IJSRuntime _jsRuntime;

		public CryptoService()
		{
		}
		public CryptoService(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}
		public void CreateEtherealKeyPair()
		{
			_etherealKeyPair = GetNewSecp256k1KeyPair();
		}
		public Secp256k1KeyPair GetNewSecp256k1KeyPair()
		{
			Random rand = new Random();
			byte[] privateKeyGen = new byte[32];
			rand.NextBytes(privateKeyGen);
			var privateKey = ECPrivKey.Create(privateKeyGen);
			var publicKey = privateKey.CreateXOnlyPubKey();
			return new Secp256k1KeyPair(privateKey, publicKey);
		}
		public virtual async Task<Tuple<string, string>> AesEncrypt(string plainText, string theirPublicKey, string? ivOverride = null)
		{
			byte[] sharedPoint = await GetSharedSecret(theirPublicKey);
			byte[] iv = new byte[16];
			if (ivOverride != null)
			{
				iv = Convert.FromBase64String(ivOverride);
			}
			else
			{
				Random rand = new Random();
				rand.NextBytes(iv);
			}
			var ivString = Convert.ToBase64String(iv);
			var paddedTextBytes = Encoding.UTF8.GetBytes(plainText).Pad();
			var encrypted = await _jsRuntime.InvokeAsync<string>("aesEncrypt", paddedTextBytes, sharedPoint, iv);
			return new Tuple<string, string>(encrypted.ToString(), ivString);
		}
		public virtual async Task<string> AesDecrypt(string base64CipherText, string theirPublicKey, string ivString)
		{
			byte[] sharedPoint = await GetSharedSecret(theirPublicKey);
			var sharedPointString = Convert.ToBase64String(sharedPoint);
			var decrypted = await _jsRuntime.InvokeAsync<string>("aesDecrypt", base64CipherText, sharedPointString, ivString);
			return decrypted;
		}
		protected async Task<byte[]> GetSharedSecret(string theirPublicKey)
		{
			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
			var theirPubKey = ECPubKey.Create(theirPublicKeyBytes);
			var ourPrivKey = await GetPrivateKey();
			return theirPubKey.GetSharedPubkey(ourPrivKey).ToBytes()[1..];
		}
		protected virtual async Task<ECPrivKey> GetPrivateKey()
		{
			return _etherealKeyPair.PrivateKey;
		}
		public string Sign(string message)
		{
			var messageHashBytes = message.SHA256Hash();
			return _etherealKeyPair.PrivateKey.SignBIP340(messageHashBytes).ToString();
		}
		public bool Verify(string signature, string message, string publicKey)
		{
			var messageHashBytes = message.SHA256Hash();
			var signatureBytes = Convert.FromHexString(signature);
			var publicKeyBytes = Convert.FromHexString(publicKey);
			var pubKey = ECXOnlyPubKey.Create(publicKeyBytes);
			SecpSchnorrSignature schnorrSignature;
			SecpSchnorrSignature.TryCreate(signatureBytes, out schnorrSignature);
			return pubKey.SigVerifyBIP340(schnorrSignature, messageHashBytes);
		}
	}
}
