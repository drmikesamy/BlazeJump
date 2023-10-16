using BlazeJump.Common.Models;
using BlazeJump.Common.Models.SubtleCrypto;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.Crypto.Bindings;
using BlazeJump.Helpers;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace BlazeJump.Common.Services.Crypto
{
	public partial class CryptoService : ICryptoService
	{
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
		public async Task GenerateAndStoreUserKeyPair()
		{
			var keyPair = GenerateKeyPair();
			await SecureStorage.Default.SetAsync("PublicKey", keyPair.PublicKey);
			await SecureStorage.Default.SetAsync("PrivateKey", keyPair.PrivateKey);
		}
		public Tuple<string, string> Encrypt(string message, string theirPublicKey, string myPrivateKey)
		{
			byte[] encryptedData;
			var sharedPoint = GetSharedSecret(theirPublicKey, myPrivateKey);

			using (Aes aesAlg = Aes.Create())
			{		
				aesAlg.GenerateIV();
				aesAlg.Mode = CipherMode.CBC;
				aesAlg.KeySize = 256;
				byte[] iv = aesAlg.IV;
				aesAlg.Padding = PaddingMode.PKCS7;
				aesAlg.BlockSize = 128;
				ICryptoTransform encryptor = aesAlg.CreateEncryptor(sharedPoint, iv);

				using (MemoryStream msEncrypt = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
						{
							swEncrypt.Write(message);
						}
						encryptedData = msEncrypt.ToArray();
					}
				}
				return new Tuple<string, string>(Convert.ToBase64String(encryptedData), Convert.ToBase64String(iv));
			}
		}

		public string Decrypt(string cipherText, string theirPublicKey, string myPrivateKey, string ivString)
		{
			byte[] cipherBytes = Convert.FromBase64String(cipherText);

			var sharedPoint = GetSharedSecret(theirPublicKey, myPrivateKey);

			byte[] iv = Convert.FromBase64String(ivString);

			using (Aes aesAlg = Aes.Create())
			{
				
				aesAlg.Mode = CipherMode.CBC;
				aesAlg.KeySize = 256;
				aesAlg.Padding = PaddingMode.PKCS7;
				aesAlg.BlockSize = 128;
				aesAlg.Key = sharedPoint;
				aesAlg.IV = iv;
				ICryptoTransform decryptor = aesAlg.CreateDecryptor(sharedPoint, iv);

				using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
				{
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader srDecrypt = new StreamReader(csDecrypt))
						{
							return srDecrypt.ReadToEnd();
						}
					}
				}
			}
		}
		public async Task<NEvent> SignEvent(NEvent nEvent, string? myPrivateKey = null)
		{
			if(myPrivateKey == null)
			{
				var keyPair = await GetUserKeyPair();
				myPrivateKey = keyPair.PrivateKey;
			}

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
			var privateKeyBytes = Convert.FromHexString(myPrivateKey);
			var signature = SecP256k1.SignCompact(eventHash, privateKeyBytes, out var recoveryKey);
			nEvent.Sig = Convert.ToHexString(signature);
			return nEvent;
		}
	}
}