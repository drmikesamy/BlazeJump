using BlazeJump.Client.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazeJump.Client.Services.Database
{
	public class BlazeDbService : IBlazeDbService
	{
		private IDbContextFactory<BlazeDbContext> _dbContextFactory { get; set; }
		public BlazeDbService(IDbContextFactory<BlazeDbContext> dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;
		}

		public BlazeDbContext Context { get; set; }

		public async Task InitDatabaseAsync()
		{
			try
			{
				Context = await _dbContextFactory.CreateDbContextAsync();
				await Context.Database.EnsureCreatedAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.GetType().Name, ex.Message);
			}
		}
	}
}
