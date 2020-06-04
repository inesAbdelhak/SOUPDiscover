using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SoupDiscover
{
    /// <summary>
    /// Configure database from parameters
    /// </summary>
    public static class ConfigurationExtention
    {
        public static DbContextOptionsBuilder UseDatabaseConfig([NotNull] this DbContextOptionsBuilder optionsBuilder, IConfiguration configuration, ILogger logger = null)
        {
            logger = logger ?? NullLogger.Instance;
            // Search configuration in environment
            var databaseType = Environment.GetEnvironmentVariable("DatabaseType");
            if (string.IsNullOrEmpty(databaseType))
            {
                // Search configuration in appsettings.json
                databaseType = configuration.GetValue("DatabaseType", SupportedDatabase.SQLite.ToString());
            }
            if (!Enum.TryParse<SupportedDatabase>(databaseType, true, out var dbType))
            {
                dbType = SupportedDatabase.SQLite;
                logger.LogInformation($"The database type {databaseType} is not recognized. Use database type {dbType} instead.");
            }
            // Search configuration in environment and in appsettings
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString") ?? configuration.GetConnectionString("Default") ?? "Data Source=CustomerDB.db;";

            logger.LogInformation($"Use database type {dbType} with connectionString {connectionString}.");
            // Configure database
            switch (dbType)
            {
                case SupportedDatabase.SQLite:
                    optionsBuilder.UseSqlite(connectionString);
                break;

                case SupportedDatabase.Postgres:
                    optionsBuilder.UseNpgsql(connectionString);
                break;

                default:
                    throw new ApplicationException("Unable to configure the database type");
            }

            return optionsBuilder;
        }
    }
}
