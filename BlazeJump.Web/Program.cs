using AutoMapper;
using BlazeJump.Common.Data;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.Database;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.Notification;
using BlazeJump.Common.Services.UserProfile;
using BlazeJump.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("BlazeJump.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ICryptoService, CryptoService>(
	(ctx) =>
	{
		var jsRuntime = ctx.GetService<IJSRuntime>();
		return new CryptoService(jsRuntime);
	});
builder.Services.AddScoped<IRelayManager, RelayManager>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IBlazeDbService, BlazeDbService>();
builder.Services.AddDbContextFactory<BlazeDbContext>(opts => opts.UseSqlite("Filename=app.db"));

var mapperConfig = new MapperConfiguration(cfg =>
{
	cfg.AddMaps(Assembly.GetExecutingAssembly());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
await builder.Build().RunAsync();