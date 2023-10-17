using BlazeJump.Common.Models;
using BlazeJump.Common.Models.SubtleCrypto;

namespace BlazeJump.Common.Services.Crypto
{
    public partial interface ICryptoService
    {
		Task<Secp256k1KeyPair> GetUserKeyPair();
		Task GenerateAndStoreUserKeyPair();
		Tuple<string, string> NativeAesEncrypt(string plainText, string theirPublicKey, string myPrivateKey, string? ivOverride = null);
		string NativeAesDecrypt(string cipherText, string theirPublicKey, string myPrivateKey, string ivString);
		Task<NEvent> SignEvent(NEvent nEvent, string? myPrivateKey = null);
	}
}