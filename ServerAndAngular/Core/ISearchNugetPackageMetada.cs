using SoupDiscover.ORM;

namespace SoupDiscover.Common
{
    public interface ISearchNugetPackageMetada
    {
        Package SearchMetadata(string packageId, string version, string[] sources);
    }
}
