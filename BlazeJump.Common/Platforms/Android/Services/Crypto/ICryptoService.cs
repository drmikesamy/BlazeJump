using BlazeJump.Common.Models;
using BlazeJump.Common.Models.Crypto;

namespace BlazeJump.Common.Services.Crypto
{
    public partial interface ICryptoService
    {
		Task GetUserKeyPair();
		Task GenerateAndStoreUserKeyPair();
		Tuple<string, string> NativeAesEncrypt(string plainText, string theirPublicKey, string? ivOverride = null);
		string NativeAesDecrypt(string cipherText, string theirPublicKey, string ivString);
	}
}