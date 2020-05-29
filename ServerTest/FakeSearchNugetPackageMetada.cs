using SoupDiscover.Common;
using SoupDiscover.ORM;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class FakeSearchNugetPackageMetada : ISearchNugetPackage
{
    public Package SearchMetadata(string packageId, string version, string[] sources, CancellationToken token)
    {
        return new Package() { PackageId = packageId, Version = version, PackageType = PackageType.Npm, Description = $"{packageId}@{version}" };
    }

    public async Task<IEnumerable<PackageConsumerName>> SearchPackages(string path, CancellationToken token = default)
    {
        return new PackageConsumerName[]
        {
            new PackageConsumerName("monCsproj", new []
            {
                new PackageName("log4Net", "2.0.8", PackageType.Nuget),
            }),
         };
    }
}