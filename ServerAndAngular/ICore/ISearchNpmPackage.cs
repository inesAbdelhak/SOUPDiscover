using SoupDiscover.ORM;
using System.Threading;
using System.Threading.Tasks;
using SoupDiscover.Core;

namespace SoupDiscover.ICore
{
    /// <summary>
    /// Search packages
    /// </summary>
    public interface ISearchPackage
    {
        public const string NoneLicenseExpression = "None";

        /// <summary>
        /// Search a nuget package MetaData
        /// </summary>
        /// <param name="packageId">The package id to search</param>
        /// <param name="version">The version of the package to search</param>
        Package SearchMetadata(string packageId, string version, SearchPackageConfiguration configuration, CancellationToken token = default);

        /// <summary>
        /// Search packages without there metadata
        /// </summary>
        /// <param name="checkoutDirectory">The directory where the repository is checkout</param>
        /// <param name="token">The token to cancel the processing</param>
        /// <returns></returns>
        Task<PackageConsumerName[]> SearchPackages(string checkoutDirectory, CancellationToken token = default);

        /// <summary>
        /// Type of package to search
        /// </summary>
        PackageType PackageType { get; }
    }
}
