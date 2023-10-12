using BlazeJump.Common.Models;
using BlazeJump.Common.Models.SubtleCrypto;

namespace BlazeJump.Common.Services.Crypto
{
    public interface ICryptoService
    {
		string ECDHPublicKey { get; set; }
        string ECDHPrivateKey { get; set; }
        Task<Secp256k1KeyPair> GenerateKeyPair();
        Task<NEvent> SignEvent(NEvent nEvent);
		Task<string> AesEncrypt(string plainText, string theirPublicKey, string myPrivateKey);
		//Task<string> Encrypt(string pubKey, string plainText);
		//Task<string> Decrypt(string pubKey, string cipherText);

	}
}