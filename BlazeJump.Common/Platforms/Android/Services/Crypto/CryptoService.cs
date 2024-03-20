using System.Security.Cryptography;

namespace BlazeJump.Common.Services.Crypto
{
	public partial class CryptoService : ICryptoService
	{
		public async Task GetUserKeyPair()
		{
			if(_keyPair.PublicKey == null)
			{
				await GenerateAndStoreUserKeyPair();
			}

			_keyPair.PrivateKeyString = await SecureStorage.Default.GetAsync("PrivateKey");
		}
		public async Task GenerateAndStoreUserKeyPair()
		{
			GenerateKeyPair();
			await SecureStorage.Default.SetAsync("PublicKey", _keyPair.PublicKeyString);
			await SecureStorage.Default.SetAsync("PrivateKey", _keyPair.PrivateKeyString);
		}
		public Tuple<string, string> NativeAesEncrypt(string message, string theirPublicKey, string? ivOverride = null)
		{
			byte[] encryptedData;
			var sharedPoint = GetSharedSecret(theirPublicKey);

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

		public string NativeAesDecrypt(string cipherText, string theirPublicKey, string ivString)
		{
			byte[] cipherBytes = Convert.FromBase64String(cipherText);

			var sharedPoint = GetSharedSecret(theirPublicKey);

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
	}
}