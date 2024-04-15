using AutoMapper;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.Notification;
using BlazeJump.Common.Services.UserProfile;
using Microsoft.Extensions.Logging;
using System.Reflection;
using ZXing.Net.Maui.Controls;
using BlazeJump.Common.Services.Identity;

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

			builder.Services.AddScoped<IUserProfileService, UserProfileService>();
			builder.Services.AddScoped<IMessageService, MessageService>();
#if ANDROID
		builder.Services.AddScoped<ICryptoService, NativeCryptoService>();
#endif
			builder.Services.AddScoped<IRelayManager, RelayManager>();
			builder.Services.AddScoped<INotificationService, NotificationService>();
			builder.Services.AddScoped<IIdentityService, IdentityService>();
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddMaps(Assembly.GetExecutingAssembly());
			});

			IMapper mapper = mapperConfig.CreateMapper();
			builder.Services.AddSingleton(mapper);

			return builder.Build();
		}
	}
}