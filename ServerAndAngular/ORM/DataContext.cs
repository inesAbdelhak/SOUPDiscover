using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Package>().HasIndex(p => p.PackageId);
        }

        public DbSet<GitRepository> GitRepositories { get; set; }

        public DbSet<Credential> Credentials { get; set; }

        public DbSet<Package> Packages { get; set; }

        public DbSet<SOUPSearchProject> Projects { get; set; }

        public DbSet<Repository> Repositories { get; set; }

        public DbSet<PackageConsumer> PackageConsumer { get; set; }

        public DbSet<PackageConsumerPackage> PackageConsumerPackages { get; set; }
    }
}
