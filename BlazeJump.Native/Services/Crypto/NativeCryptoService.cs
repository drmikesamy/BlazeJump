using BlazeJump.Common.Models;
using BlazeJump.Common.Models.SubtleCrypto;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.Crypto.Bindings;
using BlazeJump.Helpers;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace BlazeJump.Native.Services.Crypto
{
	public class NativeCryptoService : CryptoService
	{
		public async Task GenerateAndStoreUserKeyPair()
		{
			var keyPair = await GenerateKeyPair();
			await SecureStorage.Default.SetAsync("PublicKey", keyPair.PublicKey);
			await SecureStorage.Default.SetAsync("PrivateKey", keyPair.PrivateKey);
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
			var eventHash = JsonConvert.SerializeObject(eventToSign).SHA256Hash();
			var eventHashString = eventHash.ToHashString();
			nEvent.Id = eventHashString;
			var userKeys = await GetUserKeyPair();
			var privateKeyBytes = Convert.FromHexString(userKeys.PrivateKey);
			var signature = SecP256k1.SignCompact(eventHash, privateKeyBytes, out var recoveryKey);
			nEvent.Sig = Convert.ToHexString(signature);
			return nEvent;
		}
		public async Task<Secp256k1KeyPair> GetUserKeyPair()
		{
			var publicKeyString = await SecureStorage.Default.GetAsync("PublicKey");
			var privateKeyString = await SecureStorage.Default.GetAsync("PrivateKey");

			return new Secp256k1KeyPair
			{
				PublicKey = publicKeyString,
				PrivateKey = privateKeyString
			};
		}

		public async Task<string> Encrypt(string plainText, string theirPublicKey)
		{
			byte[] encryptedData;

			var userKeyPair = await GetUserKeyPair();
			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
			var myPrivateKeyBytes = Convert.FromHexString(userKeyPair.PrivateKey);
			var sharedPoint = SecP256k1.EcdhSerialized(theirPublicKeyBytes, myPrivateKeyBytes);
			var sharedX = sharedPoint[1..];

			Random rand = new Random();
			byte[] iv = new byte[16];
			rand.NextBytes(iv);

			using (Aes aesAlg = Aes.Create())
			{
				ICryptoTransform encryptor = aesAlg.CreateEncryptor();
				aesAlg.Mode = CipherMode.CBC;
				aesAlg.Key = sharedX;
				aesAlg.KeySize = 256;
				aesAlg.IV = iv;

				using (MemoryStream msEncrypt = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
						{
							swEncrypt.Write(plainText);
						}
						encryptedData = msEncrypt.ToArray();
					}
				}
				return Convert.ToBase64String(encryptedData);
			}
		}

		public async Task<string> Decrypt(string cipherText, string theirPublicKey)
		{
			string plaintext = null;

			var userKeyPair = await GetUserKeyPair();
			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
			var myPrivateKeyBytes = Convert.FromHexString(userKeyPair.PrivateKey);
			var sharedPoint = SecP256k1.EcdhSerialized(theirPublicKeyBytes, myPrivateKeyBytes);
			var sharedX = sharedPoint[1..];

			Random rand = new Random();
			byte[] iv = new byte[16];
			rand.NextBytes(iv);

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = sharedX;

				ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
				var cipherTextBytes = Convert.FromBase64String(cipherText);

				using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
				{
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader srDecrypt = new StreamReader(csDecrypt))
						{
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}
			}

			return plaintext;
		}
	}
}