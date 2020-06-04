using Microsoft.Extensions.Logging;
using SoupDiscover.ORM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SoupDiscover.Common
{

    /// <summary>
    /// Search nuget packages
    /// </summary>
    public class SearchNpmPackage : ISearchPackage
    {
        public SearchNpmPackage(ILogger<SearchNpmPackage> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The name of the file that contains the version
        /// </summary>
        public const string PackageLockJsonFilename = "package-lock.json";
        public const string NodeModulesDirName = "node_modules";

        private (string CheckoutDirectory, string[] packageLockJson) _lastSearch;
        private ILogger<SearchNpmPackage> _logger;

        public PackageType PackageType => PackageType.Npm;

        /// <summary>
        /// Search a nuget package MetaData
        /// </summary>
        /// <param name="packageId">The package id to search</param>
        /// <param name="version">The version of the package to search</param>
        /// <param name="checkoutDirectory">The directory where the sources files are checkout</param>
        public Package SearchMetadata(string packageId, string version, SearchPackageConfiguration configuration, CancellationToken token = default)
        {
            string[] packageLockJsonFiles;
            if (_lastSearch.CheckoutDirectory == configuration.CheckoutDirectory)
            {
                packageLockJsonFiles = _lastSearch.packageLockJson;
            }
            else
            {
                // Search files packageLockJson
                packageLockJsonFiles = Directory.GetFiles(configuration.CheckoutDirectory, PackageLockJsonFilename, SearchOption.AllDirectories);
                _lastSearch = (configuration.CheckoutDirectory, packageLockJsonFiles);
            }

            // Search npm Package in "node_modules" directories
            foreach (var packageLockJson in packageLockJsonFiles)
            {
                token.ThrowIfCancellationRequested();
                var nodeModuleDir = Path.Combine(Path.GetDirectoryName(packageLockJson), NodeModulesDirName);
                var packageMetadataFile = nodeModuleDir;
                foreach (var packageElementName in packageId.Split('/'))
                {
                    packageMetadataFile = Path.Combine(packageMetadataFile, packageElementName);
                }
                packageMetadataFile = Path.Combine(packageMetadataFile, "package.json");


                if (File.Exists(packageMetadataFile))
                {
                    var json = JsonDocument.Parse(File.ReadAllText(packageMetadataFile, Encoding.UTF8));
                    var foundVersion = json.RootElement.GetProperty("version").GetString();
                    if (foundVersion != version)
                    {
                        continue; // Search version in another "node_module" directory
                    }
                    return new Package()
                    {
                        PackageId = packageId,
                        Version = version,
                        Licence = json.RootElement.TryGetValueAsString("license", "type"),
                        PackageType = PackageType.Npm,
                        Description = json.RootElement.TryGetValueAsString("description"),
                    };
                }
            }
            _logger.LogDebug($"Unable to find metadata for npm package {packageId}@{version}");
            return new Package()
            {
                PackageId = packageId,
                Version = version,
                PackageType =
                PackageType.Npm
            };
        }

        /// <summary>
        /// Search npm package metadata
        /// </summary>
        /// <param name="checkoutDirectory">The directory where the repository is checkout</param>
        /// <returns>The package with metadata</returns>
        public async Task<PackageConsumerName[]> SearchPackages(string checkoutDirectory, CancellationToken token = default)
        {
            var packageConsumers = new List<PackageConsumerName>();
            // Search all lock files
            foreach (var lockFile in Directory.GetFiles(checkoutDirectory, PackageLockJsonFilename, SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
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
                packageConsumers.Add(new PackageConsumerName(Path.GetRelativePath(checkoutDirectory, Path.GetDirectoryName(lockFile)), packages.ToArray()));
            }
            return packageConsumers.ToArray();
        }
    }
}
