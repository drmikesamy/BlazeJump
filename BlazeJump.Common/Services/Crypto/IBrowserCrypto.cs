namespace BlazeJump.Common.Services.Crypto
{
	public interface IBrowserCrypto
	{
		Task<string> InvokeBrowserCrypto(string functionName, params object[] args);
	}
}
