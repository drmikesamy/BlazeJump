using BlazeJump.Common.Models;
using BlazeJump.Common.Models.SubtleCrypto;
using BlazeJump.Common.Services.Crypto.Bindings;
using Microsoft.JSInterop;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlazeJump.Common.Services.Crypto
{
	public class CryptoService : ICryptoService
	{
		public string ECDHPublicKey { get; set; } = "";
		public string ECDHPrivateKey { get; set; } = "";
		public CryptoService()
		{
		}

		public byte[] GeneratePrivateKey()
		{
			Random rand = new Random();
			byte[] privateKey = new byte[32];
			rand.NextBytes(privateKey);
			bool validPrivateKey = SecP256k1.VerifyPrivateKey(privateKey);
			if (!validPrivateKey)
			{
				return GeneratePrivateKey();
			}
			else
			{
				return privateKey;
			}
			
		}
		public async Task<Secp256k1KeyPair> GenerateKeyPair()
		{
			var secpKeyPair = new Secp256k1KeyPair();

			var privateKeyBytes = GeneratePrivateKey();

			secpKeyPair.PrivateKey = Convert.ToHexString(privateKeyBytes);
			var publicKeyBytes = SecP256k1.GetPublicKey(privateKeyBytes, true);
			byte[] publicKeyBytes32 = publicKeyBytes[1..];

			secpKeyPair.PublicKey = Convert.ToHexString(publicKeyBytes32).ToLower();


			return secpKeyPair;
		}

		public async Task<NEvent> SignEvent(NEvent nEvent)
		{
			var eventToSign = new
			{
				kind = nEvent.Kind,
				content = nEvent.Content,
				tags = nEvent.Tags,
				pubkey = nEvent.Pubkey,
				created_at = nEvent.Created_At,
				id = 0
			};

			var signed = new NEvent();
			return signed;
		}

		public Tuple<string, byte[]> AesEncrypt(string plainText, string theirPublicKey, string myPrivateKey)
		{
			byte[] sharedPoint = GetSharedSecret(theirPublicKey, myPrivateKey);
			string sharedPointString = Convert.ToBase64String(sharedPoint);
			Random rand = new Random();
			byte[] iv = new byte[16];
			rand.NextBytes(iv);
			string ivString = Convert.ToHexString(iv);
			var encrypted = TinyAes.Encrypt(plainText, sharedPoint, iv);
			return new Tuple<string, byte[]>(Convert.ToBase64String(encrypted), iv);
		}

		public string AesDecrypt(string base64CipherText, string theirPublicKey, string myPrivateKey, byte[] iv)
		{
			byte[] sharedPoint = GetSharedSecret(theirPublicKey, myPrivateKey);
			string sharedPointString = Convert.ToBase64String(sharedPoint);
			var decrypted = TinyAes.Decrypt(base64CipherText, sharedPoint, iv);
			return Encoding.UTF8.GetString(decrypted);
		}

		private byte[] GetSharedSecret(string theirPublicKey, string myPrivateKey)
		{
			var myPrivateKeyBytes = Convert.FromHexString(myPrivateKey);
			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
			byte[] theirPublicKeyBytes33 = new byte[33];
			theirPublicKeyBytes33[0] = 0x02;
			Array.Copy(theirPublicKeyBytes, 0, theirPublicKeyBytes33, 1, 32);
			var theirPublicKeyBytesDecompressed = SecP256k1.Decompress(theirPublicKeyBytes33);
			byte[] theirPublicKeyBytesDecompressed64 = new byte[64];
			theirPublicKeyBytesDecompressed64 = theirPublicKeyBytesDecompressed[1..];
			return SecP256k1.EcdhSerialized(theirPublicKeyBytesDecompressed64, myPrivateKeyBytes);
		}
	}
}
