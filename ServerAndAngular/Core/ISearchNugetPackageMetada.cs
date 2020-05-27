using SoupDiscover.ORM;

namespace SoupDiscover.Common
{
    /// <summary>
    /// Search nuget package metadata
    /// </summary>
    public interface ISearchNugetPackageMetada
    {
        Package SearchMetadata(string packageId, string version, string[] sources);
    }
}
