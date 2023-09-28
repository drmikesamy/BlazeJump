using BlazeJump.Client.Models;
using BlazeJump.Client.Models.SubtleCrypto;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using BlazeJump.Services.Crypto.Bindings;

namespace BlazeJump.Client.Services.Crypto
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
		public void GenerateKeys()
		{
			byte[] privateKey = new byte[32];
			privateKey[0] = 1;
			bool result = SecP256k1.VerifyPrivateKey(privateKey);
		}
		public async Task<RSAKeyPair> GenerateRSAKeyPair()
		{
			var rsaJson = await _jsRuntime.InvokeAsync<string>("jsMethod");
			var rsaKeyPair = JsonConvert.DeserializeObject<RSAKeyPair>(rsaJson);
			return rsaKeyPair;
		}
		public async Task<string> GetPublicKey()
		{
			string? pubKey;
			try
			{
				return await _jsRuntime.InvokeAsync<string>("nostr.getPublicKey");
			}
			catch
			{
				if(!string.IsNullOrEmpty(ECDHPublicKey))
				{
					return ECDHPublicKey;
				}
				else
				{
					await GenerateRSAKeyPair();
					return ECDHPublicKey;
				}
			}
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
