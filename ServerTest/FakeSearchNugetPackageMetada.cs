using SoupDiscover.Common;
using SoupDiscover.ICore;
using SoupDiscover.ORM;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class FakeSearchNugetPackageMetada : ISearchPackage
{
    public PackageType PackageType => PackageType.Nuget;

    public Package SearchMetadata(string packageId, string version, SearchPackageConfiguration configuration, CancellationToken token)
    {
        return new Package() { PackageId = packageId, Version = version, PackageType = PackageType.Npm, Description = $"{packageId}@{version}" };
    }

    public async Task<PackageConsumerName[]> SearchPackages(string checkoutDirectory, CancellationToken token = default)
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