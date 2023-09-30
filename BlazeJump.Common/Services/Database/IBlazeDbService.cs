using BlazeJump.Common.Data;

namespace BlazeJump.Common.Services.Database
{
	public interface IBlazeDbService
	{
		BlazeDbContext Context { get; set; }
		Task InitDatabaseAsync();
	}
}
