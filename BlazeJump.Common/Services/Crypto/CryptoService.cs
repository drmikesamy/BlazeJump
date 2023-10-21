using BlazeJump.Common.Models.SubtleCrypto;
using BlazeJump.Common.Services.Crypto.Bindings;
using System.Text;

namespace BlazeJump.Common.Services.Crypto
{
	public partial class CryptoService : ICryptoService
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
		public Secp256k1KeyPair GenerateKeyPair()
		{
			var secpKeyPair = new Secp256k1KeyPair();

			var privateKeyBytes = GeneratePrivateKey();

			secpKeyPair.PrivateKey = Convert.ToHexString(privateKeyBytes);
			var publicKeyBytes = SecP256k1.GetPublicKey(privateKeyBytes, true);
			byte[] publicKeyBytes32 = publicKeyBytes[1..];

			secpKeyPair.PublicKey = Convert.ToHexString(publicKeyBytes32).ToLower();


			return secpKeyPair;
		}
		public Tuple<string, string> AesEncrypt(string plainText, string theirPublicKey, string myPrivateKey, string? ivOverride = null)
		{
			byte[] sharedPoint = GetSharedSecret(theirPublicKey, myPrivateKey);
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
		public string AesDecrypt(string base64CipherText, string theirPublicKey, string myPrivateKey, string ivString)
		{
			byte[] iv = Convert.FromBase64String(ivString);
			byte[] sharedPoint = GetSharedSecret(theirPublicKey, myPrivateKey);
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
		public async Task SignerPhoneHandshake()
		{
			//web - generates temporary keypair and put into QR code
			//web - starts listening for phone confirmation on nostr network
			//phone - scan QR code and go to URL which launches app to specific route with web pubkey
			//phone - generates temporary keypair, generates shared secret, encrypts response message, signs it and publishes it to nostr network
			//web - receives message from phone, event triggered to show LOGGED IN to web user
			//web - user sends a messge: message is stringified, encrypted using temp shared secret, published as ethereal message
			//phone - receives message, decrypts it, signs it, and publishes it
		}

	}
}
