using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using SoupDiscover.Core.Respository;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// The context of the database
    /// </summary>
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=CustomerDB.db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SOUPSearchProject>()
            .Property<string>("_sOUPTypeToSearchCollection")
            .HasField("_sOUPTypeToSearchCollection");
        }

        public DbSet<GitRepository> Repositories { get; set; }

        public DbSet<Credential> Credentials { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<SOUPSearchProject> Projects { get; set; }

        public DbSet<Repository> Repository { get; set; }
    }
}
