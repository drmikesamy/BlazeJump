using Xunit.Runners.Maui;
using BlazeJump.Common.Services.Crypto;
using Microsoft.JSInterop;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.ConfigureTests(new TestOptions()
		{
			Assemblies =
		{
					typeof(MauiProgram).Assembly
		}})
		.UseVisualRunner();
		builder.Services.AddScoped<ICryptoService, CryptoService>(
			(ctx) =>
			{
				var jsRuntime = ctx.GetService<IJSRuntime>();
				return new CryptoService(jsRuntime);
			});
		return builder.Build();
	}
}