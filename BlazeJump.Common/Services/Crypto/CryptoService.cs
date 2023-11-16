using BlazeJump.Common.Models.Crypto;
using BlazeJump.Common.Services.Crypto.Bindings;
using BlazeJump.Helpers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BlazeJump.Common.Services.Crypto
{
	public partial class CryptoService : ICryptoService
	{
		private Secp256k1KeyPair _keyPair { get; set; } = new();
		public byte[] PubliccKeyBytes => _keyPair.PublicKeyBytes;
		public string PublicKey { get => _keyPair.PublicKey; }
		public CryptoService()
		{
		}
		public bool GeneratePrivateKey()
		{
			Random rand = new Random();
			byte[] privateKey = new byte[32];
			rand.NextBytes(privateKey);
			bool validPrivateKey = SecP256k1.VerifyPrivateKey(privateKey);
			if (!validPrivateKey)
			{
				return false;
			}
			else
			{
				_keyPair.PrivateKeyBytes = privateKey;
				return true;
			}
			
		}
		public bool GenerateKeyPair()
		{
			var validPrivateKey = GeneratePrivateKey();

			if (!validPrivateKey)
			{
				return false;
			}
			var publicKeyBytes = SecP256k1.GetPublicKey(_keyPair.PrivateKeyBytes, true);
			_keyPair.PublicKeyBytes = publicKeyBytes[1..];
			return true;
		}
		public Tuple<string, string> AesEncrypt(string plainText, string theirPublicKey, string? ivOverride = null)
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
			var encrypted = TinyAes.Encrypt(plainText, sharedPoint, iv);
			return new Tuple<string, string>(Convert.ToBase64String(encrypted), Convert.ToBase64String(iv));
		}
		public string AesDecrypt(string base64CipherText, string theirPublicKey, string ivString)
		{
			byte[] iv = Convert.FromBase64String(ivString);
			byte[] sharedPoint = GetSharedSecret(theirPublicKey);
			var decrypted = TinyAes.Decrypt(base64CipherText, sharedPoint, iv);
			return Encoding.UTF8.GetString(decrypted);
		}
		private byte[] GetSharedSecret(string theirPublicKey)
		{
			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
			byte[] theirPublicKeyBytes33 = new byte[33];
			theirPublicKeyBytes33[0] = 0x02;
			Array.Copy(theirPublicKeyBytes, 0, theirPublicKeyBytes33, 1, 32);
			var theirPublicKeyBytesDecompressed = SecP256k1.Decompress(theirPublicKeyBytes33);
			byte[] theirPublicKeyBytesDecompressed64 = new byte[64];
			theirPublicKeyBytesDecompressed64 = theirPublicKeyBytesDecompressed[1..];
			return SecP256k1.EcdhSerialized(theirPublicKeyBytesDecompressed64, _keyPair.PrivateKeyBytes);
		}
		public string Sign(string message)
		{
			var messageHashBytes = message.SHA256Hash();
			var signature = SecP256k1.SchnorrSign(messageHashBytes, _keyPair.PrivateKeyBytes);
			return Convert.ToHexString(signature);
		}
		public bool Verify(string signature, string message, string publicKey)
		{
			var messageHashBytes = message.SHA256Hash();
			var signatureBytes = Convert.FromHexString(signature);
			var publicKeyBytes = Convert.FromHexString(publicKey);
			var v = SecP256k1.SchnorrVerify(signatureBytes, messageHashBytes, publicKeyBytes);
			return v;
		}
	}
}
