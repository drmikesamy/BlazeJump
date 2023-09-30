using AutoMapper;
using BlazeJump.Common.Data;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.Database;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.UserProfile;
using BlazeJump.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("BlazeJump.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<IRelayManager, RelayManager>();
// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BlazeJump.ServerAPI"));

builder.Services.AddSingleton<IBlazeDbService, BlazeDbService>();
builder.Services.AddDbContextFactory<BlazeDbContext>(opts => opts.UseSqlite("Filename=app.db"));

var mapperConfig = new MapperConfiguration(cfg =>
{
	cfg.AddMaps(Assembly.GetExecutingAssembly());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
await builder.Build().RunAsync();