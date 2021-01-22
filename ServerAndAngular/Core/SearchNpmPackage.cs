using Microsoft.Extensions.Logging;
using SoupDiscover.ICore;
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
        private readonly ILogger<SearchNpmPackage> _logger;

        public PackageType PackageType => PackageType.Npm;

        public static (string License, LicenseType LicenseType) GetLicense(JsonElement packageElement, string packageInstallDir)
        {
            var licenseExpression = packageElement.TryGetValueAsString("license", "type");
            if (licenseExpression != null)
            {
                return (licenseExpression, LicenseType.Expression);
            }
            var licenseUrl = packageElement.TryGetValueAsString("license", "url");
            if (licenseUrl != null)
            {
                return (licenseUrl, LicenseType.Url);
            }
            var license = packageElement.TryGetValueAsString("license");
            if (license != null)
            {
                if(license == "UNLICENSED")
                {
                    return (ISearchPackage.NoneLicenseExpression, LicenseType.None);
                }
                if (license.StartsWith("SEE LICENSE IN "))
                {
                    var licenseFile = license.Substring("SEE LICENSE IN ".Length).Trim();
                    licenseFile = Path.Combine(packageInstallDir, licenseFile.Replace('/', Path.DirectorySeparatorChar));
                    // Read the license file content
                    if (!File.Exists(licenseFile))
                    {
                        return (ISearchPackage.NoneLicenseExpression, LicenseType.None);
                    }
                    return (File.ReadAllText(licenseFile), LicenseType.File);
                }
                return (license, LicenseType.Expression);
            }
            return (ISearchPackage.NoneLicenseExpression, LicenseType.None);
        }

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
                var packageMetadataDir = nodeModuleDir;
                foreach (var packageElementName in packageId.Split('/'))
                {
                    packageMetadataDir = Path.Combine(packageMetadataDir, packageElementName);
                }
                var packageMetadataFile = Path.Combine(packageMetadataDir, "package.json");

                if (File.Exists(packageMetadataFile))
                {
                    var json = JsonDocument.Parse(File.ReadAllText(packageMetadataFile, Encoding.UTF8));
                    var foundVersion = json.RootElement.GetProperty("version").GetString();
                    if (foundVersion != version)
                    {
                        continue; // Search version in another "node_module" directory
                    }
                    json.RootElement.TryGetProperty("repository", out var repository);
                    var nullableRepository = new JsonElement?(repository);
                    var license = GetLicense(json.RootElement, packageMetadataDir);
                    return new Package()
                    {
                        PackageId = packageId,
                        Version = version,
                        License = license.License,
                        LicenseType = license.LicenseType,
                        PackageType = PackageType.Npm,
                        Description = json.RootElement.TryGetValueAsString("description"),
                        ProjectUrl = json.RootElement.TryGetValueAsString("homepage"),
                        RepositoryType = nullableRepository?.TryGetValueAsString("type"),
                        RepositoryUrl = nullableRepository?.TryGetValueAsString("url"),
                        RepositoryCommit = string.Empty,
                    };
                }
            }
            _logger.LogDebug($"Unable to find metadata for npm package {packageId}@{version}");
            return new Package()
            {
                PackageId = packageId,
                Version = version,
                PackageType = PackageType.Npm,
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
