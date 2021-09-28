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
            var connectionString = GetConnectionString(dbType, configuration);

            logger.LogInformation($"Use database type {dbType} with connectionString {connectionString}.");
            // Configure database
            switch (dbType)
            {
                case DatabaseType.SQLite:
                    optionsBuilder.UseSqlite(connectionString);
                    break;

                case DatabaseType.Postgres:
                    optionsBuilder.UseNpgsql(connectionString);
                    break;

                default:
                    throw new SoupDiscoverException("Unable to configure the database type");
            }

            return optionsBuilder;
        }

        private static string GetConnectionString(DatabaseType dbType, IConfiguration configuration)
        {
            var databaseServer = Environment.GetEnvironmentVariable("DatabaseServer");
            var databaseUser = Environment.GetEnvironmentVariable("DatabaseUser");
            var databasePassword = Environment.GetEnvironmentVariable("DatabasePassword");
            var databasePort = Environment.GetEnvironmentVariable("DatabasePort");
            var databaseName = Environment.GetEnvironmentVariable("DatabaseName");

            switch (dbType)
            {
                case DatabaseType.Postgres:
                    if (!string.IsNullOrEmpty(databaseServer) &&
                        !string.IsNullOrEmpty(databaseUser) &&
                        !string.IsNullOrEmpty(databasePassword) &&
                        !string.IsNullOrEmpty(databasePort) &&
                        !string.IsNullOrEmpty(databaseName))
                    {
                        return $"Server={databaseServer}; Port={databasePort}; Database={databaseName}; User Id={databaseUser}; Password={databasePassword};";
                    }
                    break;

                case DatabaseType.SQLite:
                    if (!string.IsNullOrEmpty(databaseName))
                    {
                        return $"Data Source={databaseName}";
                    }
                    break;
            }
            return Environment.GetEnvironmentVariable("ConnectionString") ?? configuration.GetConnectionString("Default") ?? "Data Source=CustomerDB.db;";
        }

        /// <summary>
        /// Return the database type configured
        /// Search on EnvironmentVariable and in appsetting.json file
        /// else return SQLite, the default database type
        /// </summary>
        public static DatabaseType GetDatabaseType(this IConfiguration configuration, ILogger logger = null)
        {
            logger = logger ?? NullLogger.Instance;
            var databaseType = Environment.GetEnvironmentVariable("DatabaseType");
            if (string.IsNullOrEmpty(databaseType))
            {
                // Search configuration in appsettings.json
                databaseType = configuration.GetValue("DatabaseType", DatabaseType.SQLite.ToString());
            }
            if (!Enum.TryParse<DatabaseType>(databaseType, true, out var dbType))
            {
                dbType = DatabaseType.SQLite;
                logger.LogInformation($"The database type {databaseType} is not recognized. Use database type {dbType} instead.");
            }
            return dbType;
        }
    }
}
