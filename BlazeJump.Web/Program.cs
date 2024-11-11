using AutoMapper;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.Identity;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.Notification;
using BlazeJump.Common.Services.UserProfile;
using BlazeJump.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Reflection;
using BlazeJump.Common.Services.Connections.Providers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("BlazeJump.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddSingleton<IUserProfileService, UserProfileService>();
builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddSingleton<ICryptoService, CryptoService>(
	(ctx) =>
	{
		var jsRuntime = ctx.GetService<IJSRuntime>();
		return new CryptoService(new BrowserCrypto(jsRuntime));
	});
builder.Services.AddSingleton<IRelayManager, RelayManager>(
	_ => new RelayManager(new RelayConnectionProvider()));
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddSingleton<IIdentityService, IdentityService>();

var mapperConfig = new MapperConfiguration(cfg =>
{
	cfg.AddMaps(Assembly.GetExecutingAssembly());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
await builder.Build().RunAsync();

public partial class Program { }