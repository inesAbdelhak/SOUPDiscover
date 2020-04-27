using Microsoft.EntityFrameworkCore;

namespace testAngulardotnet.ORM
{
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

        public DbSet<AuthentificationToken> AuthentificationTokens { get; set; }

        public DbSet<Package> Packages { get; set; }
        
        public DbSet<Sshkey> Sshkeys { get; set; }

        public DbSet<Project> Projects { get; set; }
    }
}
