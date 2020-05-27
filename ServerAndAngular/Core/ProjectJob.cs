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
using System.Security.AccessControl;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using SoupDiscover.Core.Respository;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using SoupDiscover.Core;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace SoupDiscover.Common
{
    /// <summary>
    /// Permit to find all SOUP in the repository defined in the project
    /// </summary>
    public class ProjectJob : IProjectJob
    {
        private readonly ILogger<ProjectJob> _logger;
        private readonly IServiceProvider _provider;
        
        private ISearchNugetPackageMetada _searchNugetPackageMetada;
        private ISearchNpmPackageMetadata _searchNpmPackageMetadata;

        public ProjectJob(ILogger<ProjectJob> logger, 
            IServiceProvider provider, 
            ISearchNugetPackageMetada searchNugetPackageMetada, 
            ISearchNpmPackageMetadata searchNpmPackageMetadata)
        {
            _searchNpmPackageMetadata = searchNpmPackageMetadata;
            _searchNugetPackageMetada = searchNugetPackageMetada;
            _logger = logger;
            _provider = provider;
        }

        /// <summary>
        /// The project to process
        /// </summary>
        public SOUPSearchProject Project { get; set; }

        public object IdJob => Project.Name;

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
            if (Project == null)
            {
                throw new ApplicationException($"The property {nameof(Project)} must be not null!");
            }
            _logger.LogInformation($"Start processing Project {Project.Name}");

            // Search nuget SOUP
            PackageConsumerName[] nugetPackages = null;
            PackageConsumerName[] npmPackages = null;
            // Copy content files of the repository to a temporary directory
            var directory = await RetrieveSourceFiles();
            // Execute command line defined in Packages.CommandLineBeforeParse
            ExecuteCommandLinesBefore(directory);
            nugetPackages = await SearchNugetPackages(directory);
            npmPackages = await SearchNpmPackages(directory);

            var list = new List<PackageConsumerName>();
            if (nugetPackages != null)
            {
                list.AddRange(nugetPackages);
            }
            if (npmPackages != null)
            {
                list.AddRange(npmPackages);
            }
            // Save all packages in database
            await SaveSearchResult(list, directory, token);
            return this;
        }

        /// <summary>
        /// Save result to database
        /// </summary>
        /// <param name="projectJob">The job that finished</param>
        private async Task SaveSearchResult(List<PackageConsumerName> packageConsumerNames, string checkoutDirectory, CancellationToken token)
        {
            var packageCache = new Dictionary<string, Package>(StringComparer.OrdinalIgnoreCase);
            var context = _provider.GetService<DataContext>();
            if (packageConsumerNames != null)
            {
                // Remove old package in the project
                context.Entry(Project).Collection(p => p.PackageConsumers).Load();
                context.PackageConsumer.RemoveRange(Project.PackageConsumers);

                // Add new packages found
                List<PackageConsumer> consumers = new List<PackageConsumer>();
                foreach (var packageConsumerName in packageConsumerNames)
                {
                    var packageConsummer = new PackageConsumer();
                    packageConsummer.Project = Project;
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
                            packageInDatabase = GetPackageWithMetadata(packageName, checkoutDirectory);
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
            context.Projects.Update(Project);
            context.SaveChanges();
        }

        /// <summary>
        /// Return the package with found metadata
        /// </summary>
        /// <param name="packageName">The package to search</param>
        /// <param name="checkoutDirectory">The directory where the repository is checkout</param>
        /// <returns></returns>
        private Package GetPackageWithMetadata(PackageName packageName, string checkoutDirectory)
        {
            switch(packageName.PackageType)
            {
                case PackageType.Nuget:
                    return _searchNugetPackageMetada.SearchMetadata(packageName.PackageId, packageName.Version, new[] { Project.NugetServerUrl });
                    
                case PackageType.Npm:
                    return _searchNpmPackageMetadata.SearchMetadata(packageName.PackageId, packageName.Version, checkoutDirectory);
                default: throw new ApplicationException($"Packages type {packageName.PackageType} is not supported!");
            }
        }

        private Package GetNpmPackageWithMetadata(string packageId, string version, string checkoutDirectory)
        {
            return new Package() { PackageId = packageId, Version = version, PackageType = PackageType.Npm };
        }

        /// <summary>
        /// Search npm package metadata
        /// </summary>
        /// <param name="directory">The directory where the repository is checkout</param>
        /// <returns>The package with metadata</returns>
        private async Task<PackageConsumerName[]> SearchNpmPackages(string directory)
        {
            var packageConsumers = new List<PackageConsumerName>();
            // Search all lock files
            foreach (var lockFile in Directory.GetFiles(directory, SearchNpmPackageMetadata.PackageLockJsonFilename, SearchOption.AllDirectories))
            {
                var packages = new HashSet<PackageName>();
                var alreadyParsed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var fileContent = File.ReadAllText(lockFile, Encoding.UTF8);
                var json = JsonDocument.Parse(fileContent);
                foreach (var dep in json.RootElement.GetProperty("dependencies").EnumerateObject())
                {
                    var packageId = dep.Name;
                    var version = dep.Value.GetProperty("version").GetString();
                    var key = $"{packageId}/{version}";

                    // Check if its a dev dependency
                    var isDev = dep.Value.TryGetProperty("dev", out var dev);
                    if (isDev)
                    {
                        isDev = dev.ValueKind == JsonValueKind.True;
                    }

                    // Doesn't add dev dependencies
                    if (!isDev && !alreadyParsed.Contains(key))
                    {
                        alreadyParsed.Add(key);
                        packages.Add(new PackageName(packageId, version, PackageType.Npm));
                    }
                }
                packageConsumers.Add(new PackageConsumerName(Path.GetRelativePath(directory, Path.GetDirectoryName(lockFile)), packages.ToArray()));
            }
            return packageConsumers.ToArray();
        }

        private async Task<PackageConsumerName[]> SearchNugetPackages(string directory)
        {
            // Search and parse files packages.assets.props and packages.config
            return SearchNugetPackagesFiles(directory).ToArray();
        }

        private IEnumerable<PackageConsumerName> SearchNugetPackagesFiles(string path)
        {
            var result = new List<PackageConsumerName>();
            var alreadyParsed = new Dictionary<string, PackageName>();
            var allPackages = new List<PackageName>();
            foreach (var jsonFile in Directory.GetFiles(path, "project.assets.json", SearchOption.AllDirectories))
            {
                string csproj = null;
                var packagesOfCurrentProject = new HashSet<PackageName>();
                try
                {
                    // Parse project.assets.json, to extract all packages
                    var jsonString = File.ReadAllText(jsonFile);
                    using (var json = JsonDocument.Parse(jsonString))
                    {
                        csproj = json.RootElement.GetProperty("project").GetProperty("restore").GetString("packagesPath");
                        var targets = json.RootElement.GetProperty("targets");
                        foreach (var package in targets.EnumerateObject().SelectMany(e => e.Value.EnumerateObject()))
                        {
                            var idAndVersion = package.Name;
                            if (alreadyParsed.TryGetValue(idAndVersion, out var foundPackage))
                            {
                                packagesOfCurrentProject.Add(foundPackage);
                            }
                            else
                            {
                                var splited = idAndVersion.Split('/');
                                var packageName = new PackageName(splited[0], splited[1], PackageType.Nuget);
                                packagesOfCurrentProject.Add(packageName);
                                alreadyParsed.Add(idAndVersion, packageName);
                            }
                        }
                    }
                    result.Add(new PackageConsumerName(csproj, packagesOfCurrentProject.ToArray()));
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Error on parsing the file {jsonFile}. Exception : {e}");
                }
            }

            var packages = new HashSet<PackageName>();
            // Parse packages.config
            foreach (var packageConfigFile in Directory.GetFiles(path, "packages.config", SearchOption.AllDirectories))
            {
                var doc = XDocument.Load(packageConfigFile);
                foreach (var pack in doc.Root.Element("packages")?.Elements())
                {
                    var id = pack.Attribute("id")?.Value;
                    var version = pack.Attribute("version")?.Value;
                    if (alreadyParsed.TryGetValue($"{id}/{version}", out var package))
                    {
                        packages.Add(package);
                    }
                    else
                    {
                        var packageName = new PackageName(id, version, PackageType.Nuget);
                        alreadyParsed.Add($"{id}/{version}", packageName);
                        packages.Add(packageName);
                    }
                }
            }
            if(packages.Any())
            {
                result.Add(new PackageConsumerName("", packages.ToArray()));
            }
            return result;
        }

        private void ExecuteCommandLinesBefore(string path)
        {
            if (string.IsNullOrEmpty(Project.CommandLinesBeforeParse))
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
            File.WriteAllText(filename, Project.CommandLinesBeforeParse);
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Update ACL
                Process.Start("chmod", $"777 {filename}").WaitForExit();
            }
            // Execute the script
            ProcessHelper.ExecuteAndLog(_logger, filename, null, path);
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
                workDir = Path.GetTempPath();
            }
            // Create a directory where working
            return Path.Combine(workDir, "Projects", $"Project{Project.Name}");
        }

        /// <summary>
        /// Return the temporary directory where files are copied
        /// </summary>
        private async Task<string> RetrieveSourceFiles()
        {
            var workDir = GetWorkDirectory();
            PathHelper.DeleteDirectory(workDir);
            var wrapperRepository = Project.Repository.GetRepositoryWrapper(_provider);
            wrapperRepository.CopyTo(workDir);
            return workDir;
        }

        Task IJob.StartAsync(CancellationToken token)
        {
            return StartAsync(token);
        }
    }
}
