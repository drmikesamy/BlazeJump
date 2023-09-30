using AutoMapper;
using BlazeJump.Common.Data;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.Database;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.UserProfile;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BlazeJump.Native
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
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
			builder.Services.AddScoped<ICryptoService, CryptoService>();
			builder.Services.AddScoped<IRelayManager, RelayManager>();
			builder.Services.AddSingleton<IBlazeDbService, BlazeDbService>();
			builder.Services.AddDbContextFactory<BlazeDbContext>(opts => opts.UseSqlite("Filename=app.db"));

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