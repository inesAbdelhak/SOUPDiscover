using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoupDiscover.ICore;
using SoupDiscover.ORM;
using System.Linq;

namespace SoupDiscover.Core
{
    /// <summary>
    /// Manage all processing job projects
    /// </summary>
    public class ProjectJobManager : JobManager, IProjectJobManager
    {
        public ProjectJobManager(ILogger<ProjectJobManager> logger)
            : base(logger)
        {
        }
    }
}
