using SoupDiscover.ORM;

namespace SoupDiscover.Common
{

    /// <summary>
    /// Search nuget packages metadata
    /// </summary>
    public class SearchNpmPackageMetadata : ISearchNpmPackageMetadata
    {
        /// <summary>
        /// Search a nuget package MetaData
        /// </summary>
        /// <param name="packageId">The package id to search</param>
        /// <param name="version">The version of the package to search</param>
        /// <param name="checkoutDirectory">The directory where the sources files are checkout</param>
        public Package SearchMetadata(string packageId, string version, string checkoutDirectory)
        {
            return new Package() { PackageId = packageId, Version = version, PackageType = PackageType.Npm };
        }
    }
}
