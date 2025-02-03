using AutoMapper;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Connections.Providers;
using BlazeJump.Common.Services.Identity;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.Notification;
using BlazeJump.Common.Services.UserProfile;

namespace BlazeJump.Common
{
	public static class CommonServices
	{
		public static void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IIdentityService, IdentityService>();
			services.AddSingleton<IMessageService, MessageService>();
			services.AddSingleton<INotificationService, NotificationService>();
			services.AddSingleton<IRelayManager, RelayManager>(
				_ => new RelayManager(new RelayConnectionProvider()));
			services.AddSingleton<IUserProfileService, UserProfileService>();

			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddMaps("BlazeJump.Common");
			});
			mapperConfig.AssertConfigurationIsValid();

			IMapper mapper = mapperConfig.CreateMapper();
			services.AddSingleton(mapper);
		}
	}
}