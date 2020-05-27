using Microsoft.Extensions.Logging;
using SoupDiscover.ORM;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SoupDiscover.Common
{

    /// <summary>
    /// Search nuget packages metadata
    /// </summary>
    public class SearchNpmPackageMetadata : ISearchNpmPackageMetadata
    {
        public SearchNpmPackageMetadata(ILogger<SearchNpmPackageMetadata> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The name of the file that contains the version
        /// </summary>
        public const string PackageLockJsonFilename = "package-lock.json";
        public const string NodeModulesDirName = "node_modules";

        private (string CheckoutDirectory, string[] packageLockJson) _lastSearch;
        private ILogger<SearchNpmPackageMetadata> _logger;

        /// <summary>
        /// Search a nuget package MetaData
        /// </summary>
        /// <param name="packageId">The package id to search</param>
        /// <param name="version">The version of the package to search</param>
        /// <param name="checkoutDirectory">The directory where the sources files are checkout</param>
        public Package SearchMetadata(string packageId, string version, string checkoutDirectory)
        {
            string[] packageLockJson = null;
            if (_lastSearch.CheckoutDirectory == checkoutDirectory)
            {
                packageLockJson = _lastSearch.packageLockJson;
            }
            else
            {
                // Search files packageLockJson
                packageLockJson = Directory.GetFiles(checkoutDirectory, PackageLockJsonFilename);
                _lastSearch = (checkoutDirectory, packageLockJson);
            }

            // Search npm Package in "node_modules" directories
            foreach (var e in packageLockJson)
            {
                var nodeModuleDir = Path.Combine(Path.GetDirectoryName(e), NodeModulesDirName);
                var packageMetadataFile = nodeModuleDir;
                foreach (var packageElementName in packageId.Split('/'))
                {
                    packageMetadataFile = Path.Combine(packageMetadataFile, packageElementName);
                }
                packageMetadataFile = Path.Combine(packageMetadataFile, "package.json");


                if (File.Exists(packageMetadataFile))
                {
                    var json = JsonDocument.Parse(File.ReadAllText(packageMetadataFile, Encoding.UTF8));
                    var foundversion = json.RootElement.GetProperty("version").GetString();
                    if(foundversion != version)
                    {
                        continue; // Search version in another "node_module" directory
                    }
                    return new Package()
                    { 
                        PackageId = packageId, 
                        Version = version, 
                        Licence = json.RootElement.GetProperty("license").GetString(), 
                        PackageType = PackageType.Npm, 
                        Description = json.RootElement.GetProperty("description").GetString(),
                    };
                }
            }
            _logger.LogDebug($"Unable to find metadata for npm package {packageId}@{version}");
            return new Package() { PackageId = packageId, Version = version, PackageType = PackageType.Npm };
        }
    }
}
