using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SoupDiscover.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SoupDiscover.Database
{
    /// <summary>
    /// Configure database from parameters
    /// </summary>
    public static class ConfigurationExtention
    {
        public static DbContextOptionsBuilder UseDatabaseConfig([NotNull] this DbContextOptionsBuilder optionsBuilder, IConfiguration configuration, ILogger logger = null)
        {
            logger = logger ?? NullLogger.Instance;

            var dbType = configuration.GetDatabaseType(logger);

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
                    throw new SoupDiscoverException("Unable to configure the database type");
            }

            return optionsBuilder;
        }

        /// <summary>
        /// Return the database type configured
        /// Search on EnvironmentVariable and in appsetting.json file
        /// else return SQLite, the default database type
        /// </summary>
        /// <returns></returns>
        public static SupportedDatabase GetDatabaseType(this IConfiguration configuration, ILogger logger = null)
        {
            logger = logger ?? NullLogger.Instance;
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
            return dbType;
        }
    }
}
