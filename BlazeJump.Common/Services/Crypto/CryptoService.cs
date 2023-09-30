using BlazeJump.Common.Models;
using BlazeJump.Common.Models.SubtleCrypto;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using BlazeJump.Common.Services.Crypto.Bindings;

namespace BlazeJump.Common.Services.Crypto
{
	public class CryptoService : ICryptoService
	{
		private IJSRuntime _jsRuntime;
		public string ECDHPublicKey { get; set; } = "";
		public string ECDHPrivateKey { get; set; } = "";

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

			var signed = await _jsRuntime.InvokeAsync<NEvent>("nostr.signEvent", eventToSign);
			return signed;
		}

		public async Task<string> Encrypt(string pubKey, string plainText)
		{
			var encrypted = await _jsRuntime.InvokeAsync<string>("nostr.nip04.encrypt", pubKey, plainText);
			return encrypted;
		}

		public async Task<string> Decrypt(string pubKey, string cipherText)
		{
			var decrypted = await _jsRuntime.InvokeAsync<string>("nostr.nip04.decrypt");
			return decrypted;
		}
	}
}
