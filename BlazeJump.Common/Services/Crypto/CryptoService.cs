using BlazeJump.Common.Models;
using BlazeJump.Common.Models.SubtleCrypto;
using BlazeJump.Common.Services.Crypto.Bindings;
using Microsoft.JSInterop;
using System.Security.Cryptography;

namespace BlazeJump.Common.Services.Crypto
{
	public class CryptoService : ICryptoService
	{
		public string ECDHPublicKey { get; set; } = "";
		public string ECDHPrivateKey { get; set; } = "";

		private IJSRuntime _jsRuntime;
		public CryptoService(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
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

		public async Task<string> AesEncrypt(string plainText, string theirPublicKey, string myPrivateKey)
		{
			byte[] encryptedData;

			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
			var myPrivateKeyBytes = Convert.FromHexString(myPrivateKey);
			var sharedPoint = SecP256k1.EcdhSerialized(theirPublicKeyBytes, myPrivateKeyBytes);
			var sharedX = sharedPoint[1..];
			var encrypted = await _jsRuntime.InvokeAsync<string>("blazeJump.aesEncrypt", sharedX, plainText);
			return encrypted;
		}

		//public async Task<string> Decrypt(string cipherText, string theirPublicKey, string ourPrivateKey)
		//{
		//	string plaintext = null;

		//	var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
		//	var myPrivateKeyBytes = Convert.FromHexString(userKeyPair.PrivateKey);
		//	var sharedPoint = SecP256k1.EcdhSerialized(theirPublicKeyBytes, myPrivateKeyBytes);
		//	var sharedX = sharedPoint[1..];

		//	Random rand = new Random();
		//	byte[] iv = new byte[16];
		//	rand.NextBytes(iv);

		//	using (Aes aesAlg = Aes.Create())
		//	{
		//		aesAlg.Key = sharedX;

		//		ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
		//		var cipherTextBytes = Convert.FromBase64String(cipherText);

		//		using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
		//		{
		//			using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
		//			{
		//				using (StreamReader srDecrypt = new StreamReader(csDecrypt))
		//				{
		//					plaintext = srDecrypt.ReadToEnd();
		//				}
		//			}
		//		}
		//	}

		//	return plaintext;
		//}
	}
}
