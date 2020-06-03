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

namespace SoupDiscover.Common
{
    /// <summary>
    /// Permit to find all SOUP in the repository defined in the project
    /// </summary>
    public class ProjectJob : IProjectJob
    {
        private readonly ILogger<ProjectJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        
        private ISearchNugetPackage _searchNugetPackage;
        private ISearchNpmPackage _searchNpmPackage;
        private ProjectDto _project;
        private RepositoryManager _repositoryManager;

        public ProjectJob(ILogger<ProjectJob> logger,
            IServiceScopeFactory scopeFactory, 
            ISearchNugetPackage searchNugetPackageMetada, 
            ISearchNpmPackage searchNpmPackageMetadata)
        {
            _searchNpmPackage = searchNpmPackageMetadata;
            _searchNugetPackage = searchNugetPackageMetada;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// The project to process
        /// </summary>
        public ProjectDto ProjectDto
        {
            get
            {
                return _project;
            }
        }

        public void SetProject(ProjectDto project, IServiceProvider provider)
        {
            _project = project;
            _repositoryManager = ProjectDto.Repository.GetRepositoryManager(provider);
        }

        public object IdJob => ProjectDto.Name;

        /// <summary>
        /// Start synchronously the process, to find all SOUP in the repository
        /// </summary>
        /// <param name="token">The token to stop the processing</param>
        public void Execute(CancellationToken token)
        {
            StartAsync(token).Wait();
        }

        /// <summary>
        /// Start asynchronously the process, to find all SOUP in the repository
        /// </summary>
        /// <param name="token">The token to stop the processing</param>
        public async Task<ProjectJob> StartAsync(CancellationToken token)
        {
            if(ProjectDto?.Repository == null)
            {
                throw new ApplicationException("The project to process must be defined");
            }
            try
            {
                return await ProcessProjectAnalyse(token);
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

        private async Task<ProjectJob> ProcessProjectAnalyse(CancellationToken token)
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
            
            var nugetPackagesTask = _searchNugetPackage.SearchPackages(directory, token);
            
            var npmPackagesTask = _searchNpmPackage.SearchPackages(directory, token);

            var list = new List<PackageConsumerName>();
            // Search nuget packages
            var nugetPackages = await nugetPackagesTask;

            // Search npm packages
            var npmPackages = await npmPackagesTask;
            if (nugetPackages != null)
            {
                list.AddRange(nugetPackages);
            }
            if (npmPackages != null)
            {
                list.AddRange(npmPackages);
            }
            // Save all packages in database
            await CreateScopeAndSave(list, directory, token);
            return this;
        }
        
        /// <summary>
        /// Create the scope dependency and save data
        /// </summary>
        private async Task CreateScopeAndSave(List<PackageConsumerName> packageConsumerNames, string checkoutDirectory, CancellationToken token)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                await SaveSearchResult(packageConsumerNames, checkoutDirectory, context, token);
            }
        }

        /// <summary>
        /// Save result to database
        /// </summary>
        /// <param name="projectJob">The job that finished</param>
        private async Task SaveSearchResult(List<PackageConsumerName> packageConsumerNames, string checkoutDirectory, DataContext context, CancellationToken token)
        {
            var packageCache = new Dictionary<string, Package>(StringComparer.OrdinalIgnoreCase);
            var project = context.Projects.Find(ProjectDto.Name);
            if (packageConsumerNames != null)
            {
                // Remove old package in the project
                context.Entry(project).Collection(p => p.PackageConsumers).Load();
                context.PackageConsumer.RemoveRange(project.PackageConsumers);

                // Add new packages found
                List<PackageConsumer> consumers = new List<PackageConsumer>();
                foreach (var packageConsumerName in packageConsumerNames)
                {
                    var packageConsummer = new PackageConsumer();
                    packageConsummer.Project = project;
                    packageConsummer.Packages = new List<PackageConsumerPackage>();
                    packageConsummer.Name = packageConsumerName.Name;
                    consumers.Add(packageConsummer);
                    foreach (var packageName in packageConsumerName.Packages)
                    {
                        // Search package in cache
                        packageCache.TryGetValue(packageName.GetSerialized(), out var packageInDatabase);
                        // Search package in database
                        packageInDatabase = packageInDatabase ?? context.Packages.Where(p => p.PackageId == packageName.PackageId && p.Version == packageName.Version).FirstOrDefault();
                        if (packageInDatabase == null)
                        {
                            // Create a package
                            packageInDatabase = GetPackageWithMetadata(packageName, checkoutDirectory, token);
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
            }
            project.LastAnalysisError = "";
            project.LastAnalysisDate = DateTime.Now;
            context.Projects.Update(project);
            context.SaveChanges();
        }

        /// <summary>
        /// Return the package with found metadata
        /// </summary>
        /// <param name="packageName">The package to search</param>
        /// <param name="checkoutDirectory">The directory where the repository is checkout</param>
        /// <returns></returns>
        private Package GetPackageWithMetadata(PackageName packageName, string checkoutDirectory, CancellationToken token = default)
        {
            switch(packageName.PackageType)
            {
                case PackageType.Nuget:
                    return _searchNugetPackage.SearchMetadata(packageName.PackageId, packageName.Version, new[] { ProjectDto.NugetServerUrl }, token);
                    
                case PackageType.Npm:
                    return _searchNpmPackage.SearchMetadata(packageName.PackageId, packageName.Version, checkoutDirectory, token);
                default: throw new ApplicationException($"Packages type {packageName.PackageType} is not supported!");
            }
        }
       

        private void ExecuteCommandLinesBefore(string path, CancellationToken token)
        {
            if (string.IsNullOrEmpty(ProjectDto.CommandLinesBeforeParse))
            {
                return; // No command line before parse is defined
            }
            string filename;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Create a temporary file in path directory
                filename = Path.Combine(path, "CommandLinesBeforeParse.sh");
            }
            else
            {
                filename = Path.Combine(path, "CommandLinesBeforeParse.bat");
            }
            File.WriteAllText(filename, ProjectDto.CommandLinesBeforeParse);
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Update ACL to be executable
                Process.Start("chmod", $"+x {filename}").WaitForExit();
            }
            // Execute the script
            ProcessHelper.ExecuteAndLog(_logger, filename, null, path, token);
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
                workDir = @"c:\temp\";
            }
            // Create a directory where working
            return Path.Combine(workDir, $"Project{ProjectDto.Name}");
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

        Task IJob.StartAsync(CancellationToken token)
        {
            return StartAsync(token);
        }
    }
}
