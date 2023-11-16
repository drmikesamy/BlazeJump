using BlazeJump.Common.Models.Crypto;

namespace BlazeJump.Common.Services.Crypto
{
    public partial interface ICryptoService
    {
		string PublicKey { get; }
		bool GenerateKeyPair();
        Tuple<string, string> AesEncrypt(string plainText, string theirPublicKey, string? ivOverride = null);
		string AesDecrypt(string base64CipherText, string theirPublicKey, string ivString);
		string Sign(string message);
		bool Verify(string signature, string message, string publicKey);
	}
}