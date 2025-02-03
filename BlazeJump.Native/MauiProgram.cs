using AutoMapper;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Native.Services.Crypto;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.Notification;
using BlazeJump.Common.Services.UserProfile;
using Microsoft.Extensions.Logging;
using System.Reflection;
using ZXing.Net.Maui.Controls;
using BlazeJump.Common.Services.Identity;
using BlazeJump.Common.Services.Connections.Providers;
using BlazeJump.Common;

namespace BlazeJump.Native
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseBarcodeReader()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			builder.Services.AddMauiBlazorWebView();

#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
#endif
			CommonServices.ConfigureServices(builder.Services);
#if ANDROID
		builder.Services.AddScoped<ICryptoService, NativeCryptoService>();
#endif

			return builder.Build();
		}
	}
}