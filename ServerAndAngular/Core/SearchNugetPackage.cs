using log4net.Core;
using Microsoft.Extensions.Logging;
using SoupDiscover.ORM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoupDiscover.Common
{
    /// <summary>
    /// Search nuget package 
    /// Search meta-data from HTTP source server
    /// Search packages used from directory source code
    /// </summary>
    public class SearchNugetPackage : ISearchNugetPackage
    {
        private Lazy<WebClient> _webClient = new Lazy<WebClient>(() => new WebClient());
        private readonly ILogger<SearchNugetPackage> _logger;

        public SearchNugetPackage(ILogger<SearchNugetPackage> logger)
        {
            _logger = logger;
        }

        public Package SearchMetadata(string packageId, string version, string[] sources, CancellationToken token = default)
        {
            if (sources == null || sources.Length == 0)
            {
                return new Package() { PackageId = packageId, Version = version, Licence = null, PackageType = PackageType.Nuget };
            }
            // Retrieve package source in
            string metadataAsXml = null;
            foreach (var source in sources.Where(e => !string.IsNullOrEmpty(e)))
            {
                try
                {
                    metadataAsXml = _webClient.Value.DownloadString($"{source}/Packages(Id='{packageId}',Version='{version}')");
                }
                catch (Exception)
                {
                }
                if (metadataAsXml != null)
                {
                    break;
                }
            }
            string licenceUrl = null;
            string description = null;
            if (metadataAsXml != null)
            {
                var document = XDocument.Parse(metadataAsXml);
                var properties = document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "properties");
                licenceUrl = properties?.Elements().FirstOrDefault(e => e.Name.LocalName == "LicenseUrl").Value;
                description = properties?.Elements().FirstOrDefault(e => e.Name.LocalName == "Description").Value;
            }
            else
            {
                _logger.LogInformation($"Unable to find Meta-data on package {packageId} version {version} on sources {string.Join(";", sources)}");
            }
            return new Package() { PackageId = packageId, Version = version, Licence = licenceUrl, PackageType = PackageType.Nuget, Description = description };
        }

        /// <summary>
        /// Search all nuget packages in a directory,
        /// by searching in "project.assets.json" and "packages.config" files
        /// </summary>
        public async Task<IEnumerable<PackageConsumerName>> SearchPackages(string path, CancellationToken token)
        {
            var allPackagesConsumers = new List<PackageConsumerName>();
            var alreadyParsed = new Dictionary<string, PackageName>();
            var packagesWithoutConsumer = new HashSet<PackageName>();

            // Search in project.assets.json
            allPackagesConsumers.AddRange(SearchPackagesFromAssetJson(path, alreadyParsed, token));

            // Search packages.config
            SearchPackagesFromPackageConfig(path, alreadyParsed, packagesWithoutConsumer, token);

            // Search in csproj files
            SearchPackagesFromCsproj(path, alreadyParsed, packagesWithoutConsumer, allPackagesConsumers, token);

            if (packagesWithoutConsumer.Any())
            {
                allPackagesConsumers.Add(new PackageConsumerName("", packagesWithoutConsumer.ToArray()));
            }
            return allPackagesConsumers;
        }

        /// <summary>
        /// Search in all csproj files
        /// </summary>
        /// <param name="path">The directory where checkout the repository</param>
        /// <param name="alreadyParsed"></param>
        /// <param name="packagesWithoutConsumer"></param>
        /// <param name="packageConsumers"></param>
        /// <param name="token"></param>
        private static void SearchPackagesFromCsproj(string path, Dictionary<string, PackageName> alreadyParsed, HashSet<PackageName> packagesWithoutConsumer, List<PackageConsumerName> packageConsumers, CancellationToken token)
        {
            foreach (var csprojFile in Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories))
            {
                var consumerName = Path.GetRelativePath(path, csprojFile);
                if (packageConsumers.Any(e => e.Name == consumerName))
                {
                    continue;
                }
                token.ThrowIfCancellationRequested();
                var doc = XDocument.Load(csprojFile);
                foreach (var pack in doc.Root.Descendants("PackageReference"))
                {
                    var id = pack.Attribute("Include")?.Value;
                    var version = pack.Attribute("Version")?.Value;
                    if (id == null || version == null)
                    {
                        continue; // ignore PackageReference without "include" or "version"
                    }
                    if (alreadyParsed.TryGetValue($"{id}/{version}", out var package))
                    {
                        packagesWithoutConsumer.Add(package);
                    }
                    else
                    {
                        var packageName = new PackageName(id, version, PackageType.Nuget);
                        alreadyParsed.Add($"{id}/{version}", packageName);
                        packagesWithoutConsumer.Add(packageName);
                    }
                }
            }
        }

        private static void SearchPackagesFromPackageConfig(string path, Dictionary<string, PackageName> alreadyParsed, HashSet<PackageName> packagesWithoutConsumer, CancellationToken token)
        {
            foreach (var packageConfigFile in Directory.GetFiles(path, "packages.config", SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                var doc = XDocument.Load(packageConfigFile);
                foreach (var pack in doc.Root.Element("packages")?.Elements())
                {
                    var id = pack.Attribute("id")?.Value;
                    var version = pack.Attribute("version")?.Value;
                    if (alreadyParsed.TryGetValue($"{id}/{version}", out var package))
                    {
                        packagesWithoutConsumer.Add(package);
                    }
                    else
                    {
                        var packageName = new PackageName(id, version, PackageType.Nuget);
                        alreadyParsed.Add($"{id}/{version}", packageName);
                        packagesWithoutConsumer.Add(packageName);
                    }
                }
            }
        }

        private IEnumerable<PackageConsumerName> SearchPackagesFromAssetJson(string path, Dictionary<string, PackageName> alreadyParsed, CancellationToken token)
        {
            var result = new List<PackageConsumerName>();
            foreach (var jsonFile in Directory.GetFiles(path, "project.assets.json", SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                string csproj = null;
                var packagesOfCurrentProject = new HashSet<PackageName>();
                try
                {
                    // Parse project.assets.json, to extract all packages
                    var jsonString = File.ReadAllTextAsync(jsonFile, token).Result;
                    using (var json = JsonDocument.Parse(jsonString))
                    {
                        csproj = json.RootElement.GetProperty("project").GetProperty("restore").GetProperty("projectPath").GetString();
                        csproj = Path.GetRelativePath(path, csproj);
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
            return result;
        }
    }
}
