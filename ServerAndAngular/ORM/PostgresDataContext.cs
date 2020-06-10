using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// A specific data context for Postgres
    /// This permit to have migration class for each database type
    /// </summary>
    public class PostgresDataContext : DataContext
    {
        public PostgresDataContext(DbContextOptions<PostgresDataContext> options, IConfiguration configuration)
            : base(options, configuration)
        {
        }
    }
}
