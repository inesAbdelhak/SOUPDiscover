using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// A specific data context for SqlLite
    /// This permit to have migration class for each database type
    /// </summary>
    public class SqliteDataContext : DataContext
    {
        public SqliteDataContext(DbContextOptions<SqliteDataContext> options, IConfiguration configuration)
            : base(options, configuration)
        {
        }
    }
}
