using Microsoft.JSInterop;

namespace BlazeJump.Common.Services.Crypto
{
	public class BrowserCrypto : IBrowserCrypto
	{
		private readonly IJSRuntime _jsRuntime;
		public BrowserCrypto(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}
		public async Task<string> InvokeBrowserCrypto(string functionName, params object[] args)
		{
			return await _jsRuntime.InvokeAsync<string>(functionName, args);
		}
	}
}
