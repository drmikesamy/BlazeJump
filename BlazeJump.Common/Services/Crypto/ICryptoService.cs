using BlazeJump.Common.Models.Crypto;
using NBitcoin.Secp256k1;

namespace BlazeJump.Common.Services.Crypto
{
    public partial interface ICryptoService
    {
		string PublicKey { get; }
		string XOnlyPublicKey { get; }
		bool GenerateKeyPair();
        Task<Tuple<string, string>> AesEncrypt(string plainText, string theirPublicKey, string? ivOverride = null);
		Task<string> AesDecrypt(string base64CipherText, string theirPublicKey, string ivString);
		string Sign(string message);
		bool Verify(string signature, string message, string publicKey);
	}
}