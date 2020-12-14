using Microsoft.Extensions.Logging;
using SoupDiscover.ORM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using SoupDiscover.Core.Respository;
using System.Text;
using SoupDiscover.Core;
using SoupDiscover.Controllers;
using SoupDiscover.ICore;
using Microsoft.CodeAnalysis;

namespace SoupDiscover.Common
{
    /// <summary>
    /// Permit to find all SOUP in the repository defined in the project
    /// </summary>
    public class ProjectJob : IProjectJob
    {
        private readonly ILogger<ProjectJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        
        private IDictionary<PackageType, ISearchPackage> _searchPackages;
        private RepositoryManager _repositoryManager;

        protected SearchPackageConfiguration SearchPackageConfiguraton { get; private set; }

        public ProjectJob(ILogger<ProjectJob> logger,
            IServiceScopeFactory scopeFactory, 
            IEnumerable<ISearchPackage> searchPackages)
        {
            _searchPackages = new Dictionary<PackageType, ISearchPackage>();
            foreach (var s in searchPackages)
            {
                if (_searchPackages.ContainsKey(s.PackageType))
                {
                    throw new ApplicationException($"It is not possible to defined two {nameof(ISearchPackage)} in injection dependencies, that process the same type of package.");
                }
                _searchPackages.Add(s.PackageType, s);
            }
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// The project to process
        /// </summary>
        public ProjectDto ProjectDto { get; private set; }

        public void SetProject(ProjectDto project, IServiceProvider provider)
        {
            ProjectDto = project;
            _repositoryManager = ProjectDto.Repository.GetRepositoryManager(provider);
            SearchPackageConfiguraton = CreateSearchConfiguraton();
        }

        private SearchPackageConfiguration CreateSearchConfiguraton()
        {
            var newConfiguration = new SearchPackageConfiguration(GetWorkDirectory());
            if (!string.IsNullOrEmpty(ProjectDto.NugetServerUrl))
            {
                newConfiguration.AddSources(PackageType.Nuget, new[] { ProjectDto.NugetServerUrl });
            }
            return newConfiguration;
        }

        public object IdJob => ProjectDto.Name;

        /// <summary>
        /// Start synchronously the process, to find all SOUP in the repository
        /// </summary>
        /// <param name="token">The token to stop the processing</param>
        public void Execute()
        {
            ExecuteAsync(CancellationToken.None).Wait();
        }

        /// <summary>
        /// Start asynchronously the process, to find all SOUP in the repository
        /// </summary>
        /// <param name="token">The token to stop the processing</param>
        public async Task<ProjectJob> ExecuteAsync(CancellationToken token)
        {
            if(ProjectDto?.Repository == null)
            {
                throw new ApplicationException("The project to process must be defined");
            }
            try
            {
                return await ProcessProject(token);
            }
            catch(Exception e) // Catch the first exception, not all parallel exceptions
            {
                using (var serviceScope = _scopeFactory.CreateScope())
                {
                    // Save error on database
                    var context = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
                    var project = context.Projects.Find(ProjectDto.Name);
                    if (token.IsCancellationRequested)
                    {
                        project.LastAnalysisError = "Dernière analyse annulée.";
                    }
                    else
                    {
                        project.LastAnalysisError = e.Message;
                    }
                    project.LastAnalysisDate = DateTime.Now;
                    context.Projects.Update(project);
                    context.SaveChanges();
                }
                throw e;
            }
        }

        private async Task<ProjectJob> ProcessProject(CancellationToken token)
        {
            if (ProjectDto == null)
            {
                throw new ApplicationException($"The property {nameof(ProjectDto)} must be not null!");
            }
            _logger.LogInformation($"Start processing Project {ProjectDto.Name}");

            // Copy content files of the repository to a temporary directory
            var directory = RetrieveSourceFiles(token);
            // Execute command line defined in Packages.CommandLineBeforeParse
            ExecuteCommandLinesBefore(directory, token);

            var tasks = new List<Task<PackageConsumerName[]>>();
            foreach (var searcher in _searchPackages.Values)
            {
                tasks.Add(searcher.SearchPackages(directory, token));
            }
            var list = new List<PackageConsumerName>();
            Task.WaitAll(tasks.ToArray());

            foreach (var t in tasks)
            {
                var resultat = t.Result;
                if (resultat != null)
                {
                    list.AddRange(resultat);
                }
            }
            // Save all packages in database and search metadata for all unknown found packages
            await CreateScopeAndSave(list, directory, token);
            return this;
        }
        
        /// <summary>
        /// Create the scope dependency and save data
        /// </summary>
        private async Task CreateScopeAndSave(ICollection<PackageConsumerName> packageConsumerNames, string checkoutDirectory, CancellationToken token)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                await SaveSearchResult(packageConsumerNames, context, token);
            }
        }

        /// <summary>
        /// Save result to database
        /// </summary>
        /// <param name="projectJob">The job that finished</param>
        private async Task SaveSearchResult(ICollection<PackageConsumerName> packageConsumerNames, DataContext context, CancellationToken token)
        {
            var project = context.Projects.Find(ProjectDto.Name);
            // Remove last search
            context.Entry(project).Collection(p => p.PackageConsumers).Load();
            context.PackageConsumer.RemoveRange(project.PackageConsumers);
            
            if (packageConsumerNames != null)
            {
                SavePackageConsumers(packageConsumerNames, context, project, token);
            }
            project.LastAnalysisError = "";
            project.LastAnalysisDate = DateTime.Now;
            context.Projects.Update(project);
            context.SaveChanges();
        }

        /// <summary>
        /// Save in the project, all package consumers found
        /// And search Metadata of all unkonwn packages
        /// </summary>
        private void SavePackageConsumers(ICollection<PackageConsumerName> packageConsumerNames, DataContext context, SOUPSearchProject project, CancellationToken token)
        {
            var packageCache = new Dictionary<string, Package>(StringComparer.OrdinalIgnoreCase);
            // Add new packages found
            List<PackageConsumer> consumers = new List<PackageConsumer>();
            foreach (var packageConsumerName in packageConsumerNames)
            {
                var packageConsummer = new PackageConsumer()
                {
                    Project = project,
                    Packages = new List<PackageConsumerPackage>(),
                    Name = packageConsumerName.Name
                };
                consumers.Add(packageConsummer);
                SavePackageInPackageConsumer(context, packageConsummer, packageConsumerName.Packages, packageCache, token);
            }
        }

        /// <summary>
        /// Save package on package consumers
        /// </summary>
        /// <param name="context">The database context to use</param>
        /// <param name="packageConsummer">The package consumer there and packages</param>
        /// <param name="packagesName">All packagename to add to packageConsumer</param>
        /// <param name="packageCache">All packages already find in database or just created. The key is {PackageId}/{Version}</param>
        /// <param name="token">The token, to cancel the processing</param>
        private void SavePackageInPackageConsumer(DataContext context, PackageConsumer packageConsummer, IEnumerable<PackageName> packagesName, IDictionary<string, Package> packageCache, CancellationToken token = default)
        {
            foreach (var packageName in packagesName)
            {
                token.ThrowIfCancellationRequested();
                // Search package in cache
                packageCache.TryGetValue(packageName.GetSerialized(), out var packageInDatabase);
                // Search package in database
                packageInDatabase = packageInDatabase ?? context.Packages.Where(p => p.PackageId == packageName.PackageId && p.Version == packageName.Version).FirstOrDefault();
                if (packageInDatabase == null)
                {
                    // Create a package
                    packageInDatabase = GetPackageWithMetadata(packageName, token);
                    context.Packages.Add(packageInDatabase);
                }
                // Add package in cache
                packageCache.TryAdd(packageName.GetSerialized(), packageInDatabase);
                var packageReference = new PackageConsumerPackage() { Package = packageInDatabase, PackageConsumer = packageConsummer };
                packageConsummer.Packages.Add(packageReference);
                context.PackageConsumerPackages.Add(packageReference);
                context.PackageConsumer.Add(packageConsummer);
            }
        }

        /// <summary>
        /// Return the package with found metadata
        /// </summary>
        /// <param name="packageName">The package to search</param>
        /// <param name="checkoutDirectory">The directory where the repository is checkout</param>
        /// <returns></returns>
        private Package GetPackageWithMetadata(PackageName packageName, CancellationToken token = default)
        {
            return _searchPackages[packageName.PackageType].SearchMetadata(packageName.PackageId, packageName.Version, SearchPackageConfiguraton, token);
        }

        /// <summary>
        /// Execute the command line, defined in the project, before searching packages in repository
        /// </summary>
        /// <param name="checkoutDirectory">The directory where repository is checkout</param>
        /// <param name="token">The token to cancel the processing</param>
        private void ExecuteCommandLinesBefore(string checkoutDirectory, CancellationToken token)
        {
            if (string.IsNullOrEmpty(ProjectDto.CommandLinesBeforeParse))
            {
                return; // No command line before parse is defined
            }
            string filename;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Create a temporary file in path directory
                filename = Path.Combine(checkoutDirectory, "CommandLinesBeforeParse.sh");
            }
            else
            {
                filename = Path.Combine(checkoutDirectory, "CommandLinesBeforeParse.bat");
            }

            using(var scriptFile = new StreamWriter(filename))
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    scriptFile.WriteLine("#!/bin/sh");
                }
                scriptFile.WriteLine(ProjectDto.CommandLinesBeforeParse);
            }
            
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Update ACL to be executable
                Process.Start("chmod", $"+x {filename}").WaitForExit();
            }
            // Execute the script
            ProcessHelper.ExecuteAndLog(_logger, filename, null, checkoutDirectory, token);
        }

        /// <summary>
        /// Return the directory where copy all files of the repository
        /// Its a temporary directory
        /// </summary>
        private string GetWorkDirectory()
        {
            var workDir = Environment.GetEnvironmentVariable("TempWork");
            if (workDir == null)
            {
                // workDir = Path.GetTempPath();
                workDir = $"{Path.DirectorySeparatorChar}temp";
            }
            // Create a directory where working
            return Path.GetFullPath(Path.Combine(workDir, $"Project{ProjectDto.Name}"));
        }

        /// <summary>
        /// Return the temporary directory where files are copied
        /// </summary>
        private string RetrieveSourceFiles(CancellationToken token = default)
        {
            var workDir = GetWorkDirectory();
            PathHelper.DeleteDirectory(workDir);
            _repositoryManager.CopyTo(workDir, token);
            return workDir;
        }

        Task<IJob> IJob.ExecuteAsync(CancellationToken token)
        {
            return ExecuteAsync(token).ContinueWith<IJob>(t => t.Result);
        }
    }
}
