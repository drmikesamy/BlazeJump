using BlazeJump.Client.Models;
using BlazeJump.Client.Models.SubtleCrypto;

namespace BlazeJump.Client.Services.Crypto
{
    public interface ICryptoService
    {
		string ECDHPublicKey { get; set; }
        string ECDHPrivateKey { get; set; }
        Task<RSAKeyPair> GenerateRSAKeyPair();
        Task<string> GetPublicKey();
        Task<NEvent> SignEvent(NEvent nEvent);
        Task<string> Encrypt(string pubKey, string plainText);
        Task<string> Decrypt(string pubKey, string cipherText);
        void GenerateKeys();

	}
}