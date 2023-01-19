using SoupDiscover.ICore;
using SoupDiscover.ORM;
using System.Threading;
using System.Threading.Tasks;
using SoupDiscover.Core;

internal class FakeSearchNugetPackageMetada : ISearchPackage
{
    public PackageType PackageType => PackageType.Nuget;

    public Task<Package> SearchMetadataAsync(string packageId, string version, SearchPackageConfiguration configuration, CancellationToken token)
    {
        return Task.FromResult(new Package { PackageId = packageId, Version = version, PackageType = PackageType.Npm, Description = $"{packageId}@{version}" });
    }

    public Task<PackageConsumerName[]> SearchPackagesAsync(string checkoutDirectory, CancellationToken token = default)
    {
        return Task.FromResult(new[]
        {
            new PackageConsumerName("monCsproj", new []
            {
                new PackageName("log4Net", "2.0.8", PackageType.Nuget),
            }),
        });
    }
}