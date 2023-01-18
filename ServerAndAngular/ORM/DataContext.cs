using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SoupDiscover.Core.Repository;
using SoupDiscover.Database;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// The context of the database
    /// </summary>
    public abstract class DataContext : DbContext
    {
        private readonly IConfiguration _configuration;
        protected DataContext(DbContextOptions options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            // The configuration of database must be in this class, otherwise migration generation with ef tool doesn't works
            optionsBuilder.UseDatabaseConfig(_configuration);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Package>().HasIndex(p => p.PackageId);
            modelBuilder.Entity<GitRepository>()
                .HasBaseType<Repository>();
            modelBuilder.Entity<Repository>()
                .HasDiscriminator()
                .HasValue<GitRepository>("git");
        }
        public DbSet<Repository> Repositories { get; set; }

        public DbSet<Credential> Credentials { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<ProjectEntity> Projects { get; set; }

        public DbSet<PackageConsumer> PackageConsumer { get; set; }

        public DbSet<PackageConsumerPackage> PackageConsumerPackages { get; set; }
    }
}
