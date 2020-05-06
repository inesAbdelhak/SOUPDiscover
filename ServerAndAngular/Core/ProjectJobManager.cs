using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        /// <summary>
        /// Return the list of processing project id
        /// </summary>
        public int[] GetProjectIds()
        {
            // The Job Id are the project Id
            return GetProcessingJobIds().Cast<int>().ToArray();
        }
    }
}
