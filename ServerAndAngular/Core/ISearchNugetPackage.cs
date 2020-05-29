using SoupDiscover.ORM;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoupDiscover.Common
{
    /// <summary>
    /// Search nuget package
    /// </summary>
    public interface ISearchNugetPackage
    {
        Package SearchMetadata(string packageId, string version, string[] sources, CancellationToken token = default);

        Task<IEnumerable<PackageConsumerName>> SearchPackages(string path, CancellationToken token = default);
    }
}
