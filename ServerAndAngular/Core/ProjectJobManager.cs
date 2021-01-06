using Microsoft.Extensions.Logging;
using SoupDiscover.ICore;

namespace SoupDiscover.Core
{
    /// <summary>
    /// Manage all processing job projects
    /// </summary>
    public class ProjectJobManager : JobManager, IProjectJobManager
    {
        public ProjectJobManager(ILogger<ProjectJobManager> logger)
            : base(logger, 1)
        {
        }
    }
}
