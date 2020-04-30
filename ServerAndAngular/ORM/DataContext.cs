using Microsoft.EntityFrameworkCore;

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
            optionsBuilder
           .UseSqlite(@"Data Source=CustomerDB.db;");
        }

        public DbSet<GitRepository> Repositories { get; set; }

        public DbSet<Credential> Credentials { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<Project> Projects { get; set; }
    }
}
