using NBitcoin.Secp256k1;
using System.Security.Cryptography;

namespace BlazeJump.Common.Services.Crypto
{
	public class NativeCryptoService : CryptoService, ICryptoService
	{
		public async Task<string> GetUserPublicKey()
		{
			var publicKey = await SecureStorage.Default.GetAsync("PublicKey");
			if (publicKey == null)
			{
				await GenerateAndStoreUserKeyPair();
				return await SecureStorage.Default.GetAsync("PublicKey");
			}
			return publicKey;
		}
		private async Task GenerateAndStoreUserKeyPair()
		{
			var newKeyPair = GetNewSecp256k1KeyPair();
			await SecureStorage.Default.SetAsync("PublicKey", Convert.ToHexString(newKeyPair.PublicKey.ToBytes()));
			await SecureStorage.Default.SetAsync("PrivateKey", Convert.ToHexString(newKeyPair.PrivateKey.sec.ToBytes()));
		}
		public override async Task<Tuple<string, string>> AesEncrypt(string message, string theirPublicKey, string ivOverride = null, bool ethereal = true)
		{
			byte[] encryptedData;
			var sharedPoint = await GetSharedSecret(theirPublicKey, ethereal);

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.GenerateIV();
				aesAlg.Mode = CipherMode.CBC;
				aesAlg.KeySize = 256;
				byte[] iv = new byte[16];
				if (ivOverride != null)
				{
					iv = Convert.FromBase64String(ivOverride);
				}
				else
				{
					iv = aesAlg.IV;
				}
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

		public override async Task<string> AesDecrypt(string cipherText, string theirPublicKey, string ivString, bool ethereal = true)
		{
			byte[] cipherBytes = Convert.FromBase64String(cipherText);

			var sharedPoint = await GetSharedSecret(theirPublicKey, ethereal);

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
		protected override async Task<ECPrivKey> GetPrivateKey(bool ethereal)
		{
			return ethereal ? _etherealKeyPair.PrivateKey : ECPrivKey.Create(Convert.FromHexString(await SecureStorage.Default.GetAsync("PrivateKey")));
		}
	}
}