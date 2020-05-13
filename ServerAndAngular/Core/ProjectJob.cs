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

namespace SoupDiscover.Core
{
    /// <summary>
    /// Permit to find all SOUP in the repository defined in the project
    /// </summary>
    public class ProjectJob : IProjectJob
    {
        private readonly ILogger<ProjectJob> _logger;
        private readonly IServiceProvider _provider;

        public ProjectJob(ILogger<ProjectJob> logger, IServiceProvider provider)
        {
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
            _logger.LogInformation($"Start processing Project {Project.Name}");

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
            // Save all packages in database
            await SaveToDataBase(list, token);
        }

        private async Task SaveToDataBase(List<Package> list, CancellationToken token)
        {
            var context = _provider.GetService<DataContext>();
            context.Packages.AddRange(list);
            await context.SaveChangesAsync(token);
        }

        private async Task<Package[]> SearchNpmPackages()
        {
            // Clone the repository
            // var directory = await RetriveSourceFiles();
            
            // Lancer la commande qui permet de générer le fichier lock
            // Parser le fichier lock
            return null;
        }

        private async Task<Package[]> SearchNugetPackages()
        {
            // Copy content files of the repository to a temporary directory
            var directory = await RetrieveSourceFiles();
            // Execute command line defined in Packages.CommandLineBeforeParse
            ExecuteCommandLinesBefore(directory);
            // Search and parse files packages.assets.props and packages.config
            return SearchNugetPackagesFiles(directory).ToArray();
        }

        private Package CreateNugetPackageDescription(string id, string version)
        {
            return new Package() { PackageId = id, Version = version };
        }

        private IEnumerable<Package> SearchNugetPackagesFiles(string path)
        {
            HashSet<string> alreadyParsed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<Package> allPackages = new List<Package>();
            foreach (var jsonFile in Directory.GetFiles(path, "project.assets.json", SearchOption.AllDirectories))
            {
                try
                {
                    // Parse project.assets.json, to extract all packages
                    var jsonString = File.ReadAllText(jsonFile);
                    using (var json = JsonDocument.Parse(jsonString))
                    {
                        var targets = json.RootElement.GetProperty("targets");
                        foreach (var package in targets.EnumerateObject().SelectMany(e => e.Value.EnumerateObject()))
                        {
                            var idAndVersion = package.Name;
                            if (!alreadyParsed.Contains(idAndVersion))
                            {
                                var splited = idAndVersion.Split('/');
                                allPackages.Add(CreateNugetPackageDescription(splited[0], splited[1]));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Error on parsing the file {jsonFile}. Exception : {e}");
                }
            }

            // Parse packages.config
            foreach (var packageConfigFile in Directory.GetFiles(path, "packages.config", SearchOption.AllDirectories))
            {
                var doc = XDocument.Load(packageConfigFile);
                foreach (var pack in doc.Root.Element("packages")?.Elements())
                {
                    var id = pack.Attribute("id")?.Value;
                    var version = pack.Attribute("version")?.Value;
                    if (!alreadyParsed.Contains($"{id}/{version}"))
                    {
                        allPackages.Add(CreateNugetPackageDescription(id, version));
                    }
                }
            }
            return allPackages;
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
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }
            var wrapperRepository = Project.Repository.GetRepositoryWrapper(_provider);
            wrapperRepository.CopyTo(workDir);
            return workDir;
        }
    }
}
