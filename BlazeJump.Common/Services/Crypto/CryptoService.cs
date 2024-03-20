using BlazeJump.Common.Models.Crypto;
using BlazeJump.Helpers;
using Microsoft.JSInterop;
using NBitcoin.Secp256k1;
using System.Text;

namespace BlazeJump.Common.Services.Crypto
{
	public partial class CryptoService : ICryptoService
	{
		private Secp256k1KeyPair _keyPair { get; set; } = new();
		public string PublicKey => _keyPair.PublicKeyString;
		public string XOnlyPublicKey => _keyPair.XOnlyPublicKeyString;
		private readonly IJSRuntime _jsRuntime;
		public CryptoService(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;

			GenerateKeyPair();
		}
		public bool GeneratePrivateKey()
		{
			Random rand = new Random();
			byte[] privateKey = new byte[32];
			rand.NextBytes(privateKey);
			_keyPair.PrivateKey = ECPrivKey.Create(privateKey);
			return true;
		}
		public bool GenerateKeyPair()
		{
			var validPrivateKey = GeneratePrivateKey();

			if (!validPrivateKey)
			{
				return false;
			}
			_keyPair.PublicKey = _keyPair.PrivateKey.CreatePubKey();
			_keyPair.XOnlyPublicKey = _keyPair.PrivateKey.CreateXOnlyPubKey();
			return true;
		}
		public async Task<Tuple<string, string>> AesEncrypt(string plainText, string theirPublicKey, string? ivOverride = null)
		{
			byte[] sharedPoint = GetSharedSecret(theirPublicKey);
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
		public async Task<string> AesDecrypt(string base64CipherText, string theirPublicKey, string ivString)
		{
			byte[] sharedPoint = GetSharedSecret(theirPublicKey);
			var sharedPointString = Convert.ToBase64String(sharedPoint);
			var decrypted = await _jsRuntime.InvokeAsync<string>("aesDecrypt", base64CipherText, sharedPointString, ivString);
			return decrypted;
		}
		private byte[] GetSharedSecret(string theirPublicKey)
		{
			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
			var theirPubKey = ECPubKey.Create(theirPublicKeyBytes);
			return theirPubKey.GetSharedPubkey(_keyPair.PrivateKey).ToBytes();
		}
		public string Sign(string message)
		{
			var messageHashBytes = message.SHA256Hash();
			return _keyPair.PrivateKey.SignBIP340(messageHashBytes).ToString();
		}
		public bool Verify(string signature, string message, string publicKey)
		{
			var messageHashBytes = message.SHA256Hash();
			var signatureBytes = Convert.FromHexString(signature);
			var sigString = Convert.ToHexString(messageHashBytes);
			var publicKeyBytes = Convert.FromHexString(publicKey);

			var pubKey = ECXOnlyPubKey.Create(publicKeyBytes);
			SecpSchnorrSignature schnorrSignature;
			SecpSchnorrSignature.TryCreate(signatureBytes, out schnorrSignature);
			return pubKey.SigVerifyBIP340(schnorrSignature, messageHashBytes);
		}
	}
}
