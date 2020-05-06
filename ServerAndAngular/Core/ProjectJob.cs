using Microsoft.Extensions.Logging;
using SoupDiscover.ORM;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoupDiscover.Core
{
    /// <summary>
    /// Permit to find all SOUP in the repository defined in the project
    /// </summary>
    public class ProjectJob : IProjectJob
    {
        private readonly ILogger<ProjectJob> _logger;

        public ProjectJob(ILogger<ProjectJob> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The project to process
        /// </summary>
        public Project Project { get; set; }

        public object IdJob => Project.Id;
        
        /// <summary>
        /// Start synchronously the process, to find all SOUP in the repository
        /// </summary>
        /// <param name="token">The token to stop the processing</param>
        public void Start(CancellationToken token)
        {
            StartAsync(token).Wait();
        }

        /// <summary>
        /// Start asynchronously the process, to find all SOUP in the repository
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken token)
        {
            if (Project == null)
            {
                throw new ApplicationException($"The property {nameof(Project)} must be not null!");
            }
            _logger.LogInformation($"Start processing Project {Project.Id}");
        }
    }
}
