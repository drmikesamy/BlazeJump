using BlazeJump.Common.Models.Crypto;
using BlazeJump.Helpers;
using Microsoft.JSInterop;
using NBitcoin.Secp256k1;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlazeJump.Common.Services.Crypto
{
	public partial class CryptoService : ICryptoService
	{
		public ECPubKey EtherealPublicKey => _etherealKeyPair?.PublicKey;
		protected Secp256k1KeyPair _etherealKeyPair { get; set; }

		private readonly IBrowserCrypto _browserCrypto;

		public CryptoService(IBrowserCrypto browserCrypto = null)
		{
			_browserCrypto = browserCrypto;
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
			var publicKey = privateKey.CreatePubKey();
			return new Secp256k1KeyPair(privateKey, publicKey);
		}
		public virtual async Task<CipherIv> AesEncrypt(string plainText, string theirPublicKey, string ivOverride = null, bool ethereal = true)
		{
			byte[] sharedPoint = await GetSharedSecret(theirPublicKey, ethereal);
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
			var encrypted = await _browserCrypto.InvokeBrowserCrypto("aesEncrypt", paddedTextBytes, sharedPoint, iv);
			return new CipherIv(encrypted.ToString(), ivString);
		}
		public virtual async Task<string> AesDecrypt(string base64CipherText, string theirPublicKey, string ivString, bool ethereal = true)
		{
			byte[] sharedPoint = await GetSharedSecret(theirPublicKey, ethereal);
			var sharedPointString = Convert.ToBase64String(sharedPoint);
			var decrypted = await _browserCrypto.InvokeBrowserCrypto("aesDecrypt", base64CipherText, sharedPointString, ivString);
			return decrypted;
		}
		protected async Task<byte[]> GetSharedSecret(string theirPublicKey, bool ethereal)
		{
			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
			var theirPubKey = ECPubKey.Create(theirPublicKeyBytes);
			var ourPrivKey = await GetPrivateKey(ethereal);
			return theirPubKey.GetSharedPubkey(ourPrivKey).ToBytes()[1..];
		}
		protected virtual async Task<ECPrivKey> GetPrivateKey(bool ethereal)
		{
			return _etherealKeyPair.PrivateKey;
		}
		public string Sign(string message, bool ethereal = true)
		{
			var messageHashBytes = message.SHA256Hash();
			return Convert.ToHexString(_etherealKeyPair.PrivateKey.SignBIP340(messageHashBytes).ToBytes());
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
	public class CipherIv
	{
		public CipherIv(string cipherText, string iv) {
			CipherText = cipherText;
			Iv = iv;
		}
		public string CipherText { get; set; }
		public string Iv { get; set; }
	}
}
