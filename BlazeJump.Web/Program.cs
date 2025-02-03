using AutoMapper;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using BlazeJump.Common;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

CommonServices.ConfigureServices(builder.Services);

builder.Services.AddSingleton<ICryptoService, CryptoService>(
	(ctx) =>
	{
		var jsRuntime = ctx.GetService<IJSRuntime>();
		return new CryptoService(new BrowserCrypto(jsRuntime));
	});

await builder.Build().RunAsync();

public partial class Program { }