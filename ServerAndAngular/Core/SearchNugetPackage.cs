using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using SoupDiscover.Common;
using SoupDiscover.ICore;
using SoupDiscover.ORM;

namespace SoupDiscover.Core
{
    /// <summary>
    /// Search nuget package 
    /// Search meta-data from HTTP source server
    /// Search packages used from directory source code
    /// </summary>
    public class SearchNugetPackage : ISearchPackage
    {
        private static readonly HttpClientHandler ClientHandler = new() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
        private readonly Lazy<HttpClient> _httpClient = new(() => new HttpClient(ClientHandler));
        private readonly ILogger<SearchNugetPackage> _logger;
        private static readonly SourceCacheContext Cache = new SourceCacheContext();
        private static readonly SourceRepository RepositoryV3 = NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

        public SoupDiscover.ORM.PackageType PackageType => ORM.PackageType.Nuget;

        public SearchNugetPackage(ILogger<SearchNugetPackage> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieve metadata of the package, in cache, in directory %userprofile%/.nuget/packages
        /// </summary>
        /// <returns>The Xml node of the nuspec file of the package</returns>
        private static XElement GetPackageMetadataOnLocalCache(string packageId, string version, string packageDirectory)
        {
            SoupDiscoverException.ThrowIfNullOrEmpty(packageId, $"{nameof(packageId)} must be not null or empty!");
            SoupDiscoverException.ThrowIfNullOrEmpty(version, $"{nameof(version)} must be not null or empty!");
            if (!File.Exists(packageDirectory))
            {
                return null; // package not found in cache
            }
            var nuspecFile = XDocument.Load(packageDirectory);
            return nuspecFile.Root.Element(XName.Get("metadata", nuspecFile.Root.Name.NamespaceName));
        }

        private static string GetNugetCacheDirectory(string packageId, string version)
        {
            SoupDiscoverException.ThrowIfNullOrEmpty(packageId, $"{nameof(packageId)} must be not null or empty!");
            SoupDiscoverException.ThrowIfNullOrEmpty(version, $"{nameof(version)} must be not null or empty!");
            var packageDirectory = $"{packageId}.{version}";
            return Path.Combine(NugetCacheDirectory, packageId, version, $"{packageId}.nuspec").ToLower(CultureInfo.InvariantCulture);
        }

        public static string NugetCacheDirectory
        {
            get
            {
                var nugetCacheDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(nugetCacheDir, ".nuget", "packages");
            }
        }

        private static (string License, LicenseType LicenseType) GetLicense(XElement nuspecElement, string packageDirectory)
        {
            SoupDiscoverException.ThrowIfNull(nuspecElement, $"{nameof(nuspecElement)} must be not null!");
            var licenseExpression = nuspecElement.Elements(XName.Get("license", nuspecElement.Name.NamespaceName)).FirstOrDefault(e => e.Attribute("type").Value == "expression")?.Value;
            if (licenseExpression != null)
            {
                return (licenseExpression, LicenseType.Expression);
            }

            var licenseFile = nuspecElement.Elements(XName.Get("license", nuspecElement.Name.NamespaceName)).FirstOrDefault(e => e.Attribute("type").Value == "file")?.Value;
            if (licenseFile != null)
            {
                licenseFile = Path.Combine(packageDirectory, licenseFile);
                if (!File.Exists(licenseFile))
                {
                    return (ISearchPackage.NoneLicenseExpression, LicenseType.None);
                }
                return (File.ReadAllText(licenseFile), LicenseType.File);
            }
            var licenseUrl = nuspecElement.Element(XName.Get("licenseUrl", nuspecElement.Name.NamespaceName))?.Value;
            if (licenseUrl != null)
            {
                return (licenseUrl, LicenseType.Url);
            }
            return (ISearchPackage.NoneLicenseExpression, LicenseType.None);
        }

        public async Task<Package> SearchPackageMetaDataAsync(string packageId, string version, SearchPackageConfiguration configuration, CancellationToken token = default)
        {
            var pkgMetadataResource = await RepositoryV3.GetResourceAsync<PackageMetadataResource>(token);
            var package = await pkgMetadataResource.GetMetadataAsync(new PackageIdentity(packageId, new NuGetVersion(version)), Cache, NullLogger.Instance, token);
            if (package != null)
            {
                return new Package
                {
                    PackageId = packageId,
                    Version = version,
                    License = package.LicenseMetadata?.License,
                    LicenseType = (package.LicenseMetadata?.Type).ConvertLicenseType(),
                    PackageType = ORM.PackageType.Nuget,
                    Description = package.Description,
                    ProjectUrl = package.ProjectUrl?.ToString(),
                    RepositoryUrl = package.PackageDetailsUrl?.ToString(),
                    RepositoryType = "git"
                };
            }

            return null;
        }

        public async Task<Package> SearchMetadataAsync(string packageId, string version, SearchPackageConfiguration configuration, CancellationToken token = default)
        {
            var result = await SearchPackageMetaDataAsync(packageId, version, configuration, token);
            if (result != null)
            {
                return result;
            }
            // Try to retrieve package metadata, by reading nuspec file in nuget cache directory
            var packageDirectory = GetNugetCacheDirectory(packageId, version);
            var nuspecMetadata = GetPackageMetadataOnLocalCache(packageId, version, packageDirectory);
            if (nuspecMetadata != null)
            {
                var license = GetLicense(nuspecMetadata, packageDirectory);
                return new Package()
                {
                    PackageId = packageId,
                    Version = version,
                    License = license.License,
                    LicenseType = license.LicenseType,
                    PackageType = ORM.PackageType.Nuget,
                    Description = nuspecMetadata.Element(XName.Get("description", nuspecMetadata.Name.NamespaceName))?.Value,
                    ProjectUrl = nuspecMetadata.Element(XName.Get("projectUrl", nuspecMetadata.Name.NamespaceName))?.Value,
                    RepositoryUrl = nuspecMetadata.Element(XName.Get("repository", nuspecMetadata.Name.NamespaceName))?.Attribute(XName.Get("url"))?.Value,
                    RepositoryType = nuspecMetadata.Element(XName.Get("repository", nuspecMetadata.Name.NamespaceName))?.Attribute(XName.Get("type"))?.Value,
                };
            }
            var sources = configuration.GetSources(PackageType);
            if (sources == null || sources.Count == 0)
            {
                return new Package
                {
                    PackageId = packageId, 
                    Version = version, 
                    License = ISearchPackage.NoneLicenseExpression, 
                    PackageType = ORM.PackageType.Nuget
                };
            }
            // Retrieve package metadata by calling REST nuget api server
            string metadataAsXml = null;
            foreach (var source in sources.Where(e => !string.IsNullOrEmpty(e)))
            {
                try
                {
                    var client = _httpClient.Value;
                    using var response = await client.GetAsync($"{source}/Packages(Id='{packageId}',Version='{version}')", token);
                    using var content = response.Content;
                    metadataAsXml = await content.ReadAsStringAsync(token);
                }
                catch (WebException)
                {
                    _logger.LogDebug($"Unable to find metadata for the package {packageId}@{version} in source {source}");
                }
                if (metadataAsXml != null)
                {
                    break;
                }
            }
            string licenseUrl = null;
            string description = null;
            string projectUrl = null;
            if (metadataAsXml != null)
            {
                try
                {
                    var document = XDocument.Parse(metadataAsXml);
                    var properties = document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "properties");
                    licenseUrl = properties?.Elements().FirstOrDefault(e => e.Name.LocalName == "LicenseUrl")?.Value;
                    description = properties?.Elements().FirstOrDefault(e => e.Name.LocalName == "Description").Value;
                    projectUrl = properties?.Elements().FirstOrDefault(e => e.Name.LocalName == "ProjectUrl")?.Value;
                }
                catch (XmlException e)
                {
                    _logger.LogError(e, "Can't get package meta data");
                }
            }
            else
            {
                _logger.LogInformation($"Unable to find Meta-data on package {packageId} version {version} on sources {string.Join(";", sources)}");
            }
            return new Package()
            {
                PackageId = packageId,
                Version = version,
                License = licenseUrl,
                LicenseType = LicenseType.Url,
                PackageType = ORM.PackageType.Nuget,
                Description = description,
                ProjectUrl = projectUrl,
            };
        }

        /// <summary>
        /// Search all nuget packages in a directory,
        /// by searching in "project.assets.json" and "packages.config" files
        /// </summary>
        public async Task<PackageConsumerName[]> SearchPackagesAsync(string path, CancellationToken token)
        {
            var allPackagesConsumers = new List<PackageConsumerName>();
            var alreadyParsed = new Dictionary<string, PackageName>();
            var packagesWithoutConsumer = new HashSet<PackageName>();

            // Search in project.assets.json
            allPackagesConsumers.AddRange(await SearchPackagesFromAssetJson(path, alreadyParsed, token));

            // Search packages.config
            SearchPackagesFromPackageConfig(path, alreadyParsed, packagesWithoutConsumer, token);

            // Search in csproj files
            SearchPackagesFromCsproj(path, alreadyParsed, packagesWithoutConsumer, allPackagesConsumers, token);

            if (packagesWithoutConsumer.Any())
            {
                allPackagesConsumers.Add(new PackageConsumerName("", packagesWithoutConsumer.ToArray()));
            }
            return allPackagesConsumers.ToArray();
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
                        var packageName = new PackageName(id, version, ORM.PackageType.Nuget);
                        alreadyParsed.Add($"{id}/{version}", packageName);
                        packagesWithoutConsumer.Add(packageName);
                    }
                }
            }
        }

        /// <summary>
        /// Search packages in packages.config files
        /// </summary>
        /// <param name="checkoutDirectory">The directory where repository is checkout</param>
        /// <param name="alreadyParsed"></param>
        /// <param name="packagesWithoutConsumer"></param>
        /// <param name="token"></param>
        private static void SearchPackagesFromPackageConfig(string checkoutDirectory, Dictionary<string, PackageName> alreadyParsed, HashSet<PackageName> packagesWithoutConsumer, CancellationToken token)
        {
            foreach (var packageConfigFile in Directory.GetFiles(checkoutDirectory, "packages.config", SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                var doc = XDocument.Load(packageConfigFile);
                var packages = doc.Root.Element("packages")?.Elements();
                if (packages == null)
                {
                    continue;
                }
                foreach (var pack in packages)
                {
                    var id = pack.Attribute("id")?.Value;
                    var version = pack.Attribute("version")?.Value;
                    if (alreadyParsed.TryGetValue($"{id}/{version}", out var package))
                    {
                        packagesWithoutConsumer.Add(package);
                    }
                    else
                    {
                        var packageName = new PackageName(id, version, ORM.PackageType.Nuget);
                        alreadyParsed.Add($"{id}/{version}", packageName);
                        packagesWithoutConsumer.Add(packageName);
                    }
                }
            }
        }

        /// <summary>
        /// Search packages in project.assets.json files
        /// </summary>
        /// <param name="checkoutDirectory"></param>
        /// <param name="alreadyParsed"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<IEnumerable<PackageConsumerName>> SearchPackagesFromAssetJson(string checkoutDirectory, Dictionary<string, PackageName> alreadyParsed, CancellationToken token)
        {
            var result = new List<PackageConsumerName>();
            foreach (var jsonFile in Directory.GetFiles(checkoutDirectory, "project.assets.json", SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                string csproj = null;
                var packagesOfCurrentProject = new HashSet<PackageName>();
                try
                {
                    // Parse project.assets.json, to extract all packages
                    var jsonString = await File.ReadAllTextAsync(jsonFile, token);
                    using (var json = JsonDocument.Parse(jsonString))
                    {
                        csproj = json.RootElement.GetProperty("project").GetProperty("restore").GetProperty("projectPath").GetString();
                        csproj = Path.GetRelativePath(checkoutDirectory, csproj);
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
                                var packageName = new PackageName(splited[0], splited[1], ORM.PackageType.Nuget);
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
