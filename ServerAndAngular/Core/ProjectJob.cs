using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SoupDiscover.ORM;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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
        public SOUPSearchProject Project { get; set; }

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
        /// <param name="token">The token to stop the processing</param>
        public async Task StartAsync(CancellationToken token)
        {
            if (Project == null)
            {
                throw new ApplicationException($"The property {nameof(Project)} must be not null!");
            }
            _logger.LogInformation($"Start processing Project {Project.Id}");

            // Search nuget SOUP
            Package[] nugetPackages = null;
            Package[] npmPackages = null;
            if (Project.SOUPTypeToSearch.Contains(SOUPToSearch.Nuget))
            {
                nugetPackages = await SearchNugetPackages();
            }

            if (Project.SOUPTypeToSearch.Contains(SOUPToSearch.npm))
            {
                npmPackages = await SearchNpmPackages();
            }
            var list = new List<Package>();
            if (nugetPackages != null)
            {
                list.AddRange(nugetPackages);
            }
            if (npmPackages != null)
            {
                list.AddRange(npmPackages);
            }
        }

        private async Task<Package[]> SearchNpmPackages()
        {
            // Clone du dépot avec l'option depth = 1 pour aller plus vite
            // Lancer la commande qui permet de générer le fichier lock
            // Parser le fichier lock
            return null;
        }

        private async Task<Package[]> SearchNugetPackages()
        {
            // Cloner le dépot (depth = 1, pour aller plus vite )
            var directory = await RetriveSourceFiles();
            // Lancer les lignes de commandes pour générer les fichiers packages.assets.props
            // Parser les fichiers packages.assets.props
            // Parser les fichiers packages.config
            // Récupérer les métadonnées des packages nuget
            return null;
        }

        /// <summary>
        /// Return the directory where copy all files of the repository
        /// Its a temporary directory
        /// </summary>
        private string GetWorkDirectory()
        {
            var workDir = Environment.GetEnvironmentVariable("TempWork");
            if(workDir == null)
            {
                workDir = Path.GetTempPath();
            }
            // Create a directory where working
            return Path.Combine(workDir, "Project", Project.Id.ToString());
        }

        /// <summary>
        /// Return the directory where files are copied
        /// </summary>
        /// <returns></returns>
        private async Task<string> RetriveSourceFiles()
        {
            var workDir = GetWorkDirectory();
            if(Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }
            var wrapperRepository = Project.Repository.GetRepositoryWrapper();
            wrapperRepository.CopyTo(workDir);
            return workDir;
        }
    }

}
