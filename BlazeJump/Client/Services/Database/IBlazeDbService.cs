using BlazeJump.Client.Data;

namespace BlazeJump.Client.Services.Database
{
	public interface IBlazeDbService
	{
		BlazeDbContext Context { get; set; }
		Task InitDatabaseAsync();
	}
}
