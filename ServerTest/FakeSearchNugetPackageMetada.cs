using SoupDiscover.Common;
using SoupDiscover.ORM;

internal class FakeSearchNugetPackageMetada : ISearchNugetPackageMetada
{
    public Package SearchMetadata(string packageId, string version, string[] sources)
    {
        return new Package() { PackageId = packageId, Version = version, PackageType = PackageType.Npm, Description = $"{packageId}@{version}" };
    }
}