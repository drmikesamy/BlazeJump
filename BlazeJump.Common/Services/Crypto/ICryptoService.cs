using BlazeJump.Common.Models;
using BlazeJump.Common.Models.SubtleCrypto;

namespace BlazeJump.Common.Services.Crypto
{
    public partial interface ICryptoService
    {
		string ECDHPublicKey { get; set; }
        string ECDHPrivateKey { get; set; }
        Secp256k1KeyPair GenerateKeyPair();
        Tuple<string, string> AesEncrypt(string plainText, string theirPublicKey, string myPrivateKey, string? ivOverride = null);
		string AesDecrypt(string base64CipherText, string theirPublicKey, string myPrivateKey, string ivString);
	}
}