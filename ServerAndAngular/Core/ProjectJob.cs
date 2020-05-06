using Microsoft.Extensions.Logging;
using SoupDiscover.ORM;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        /// <param name="token"></param>
        /// <returns></returns>
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

            if (Project.SOUPTypeToSearch.Contains(SOUPToSearch.Nuget))
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

        private Task<Package[]> SearchNpmPackages()
        {
            // Clone du dépot avec l'option depth = 1 pour aller plus vite
            //  On recherche le fichier ....
            // on install les packages npm pour qu'il génère le fichier package-lock
            // On parse le fichier package-lock
            // On intéroge le server de packages pour retrouver les metadonnées des packages
            return Task.FromResult((Package[])null);
        }

        private Task<Package[]> SearchNugetPackages()
        {
            // Clone the repository (depth = 1, pour aller plus vite )
            // Chercher les fichiers global.json
            //  - Télécharger les packages déclarés dans global.json : On récupère le fichier Packages.Soup.props qui se trouve dans le package stago.mhp.techcore.build.resources
            //    - Chercher tous les fichiers *.props dans les packages nuget de global.json (on trouve le fichier Packages.Soup.props) et on rechercke les <PackageReference Update="..." Version="..."/>
            // On cherche tous les fichiers *.props
            //   - On y recherche tout les <PackageReference Update="..." version="..."/>
            // On ouvre tous les fichiers packages.config 
            //  - On y cherche tous les <Package id="..." version="..."/>
            // On ouvre les fichiers *.csproj
            //  - On recherche les <PackageRefernce id="..." version="..." />
            // On intéroge les server de packages, pour retouver les metadonnée des packages
            return Task.FromResult((Package[])null);
        }
    }
}
