using BlazeJump.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazeJump.Common.Data
{
	public class BlazeDbContext : DbContext
	{
		public DbSet<NEvent> Events { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<EventTag> Tags { get; set; }

		public BlazeDbContext(DbContextOptions<BlazeDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<NEvent>();
			modelBuilder.Entity<User>();
			modelBuilder.Entity<EventTag>();

			base.OnModelCreating(modelBuilder);
		}
		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			options.LogTo(Console.WriteLine, LogLevel.Error)
				   .EnableDetailedErrors()
				   .EnableSensitiveDataLogging(true);
		}
	}
}