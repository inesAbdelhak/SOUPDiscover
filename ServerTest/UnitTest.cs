using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SoupDiscover.ORM;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SoupDiscover.Core;
using SoupDiscover.Core.Repository;

namespace ServerTest
{
    public class UnitTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSshConfigFiles()
        {
            var configFilename = Path.GetTempFileName();
            var config = new SshConfigFile(configFilename);
            Assert.IsTrue(config.Add("Host *", "test 5"));
            Assert.IsTrue(config.Save());
            Assert.AreEqual(@"Host *
  test 5

", File.ReadAllText(configFilename));
            Assert.IsFalse(config.Add("Host *", "test 5"));
            Assert.IsFalse(config.Save());
        }

        [Test]
        public async Task TestNugetMetadata()
        {
            var search = new SearchNugetPackage(NullLogger<SearchNugetPackage>.Instance);
            var package = await search.SearchMetadataAsync("log4net",
                "2.0.8",
                new SearchPackageConfiguration("",
                new Dictionary<PackageType, HashSet<string>>()
                {
                    { PackageType.Nuget, new HashSet<string> { @"https://www.nuget.org/api/v2" } }
                }));
            Assert.AreEqual("log4net", package.PackageId);
            Assert.AreEqual("2.0.8", package.Version);
            Assert.AreEqual("http://logging.apache.org/log4net/license.html", package.License);
            Assert.IsTrue(package.Description.StartsWith("log4net is a tool to help the programmer output "));
        }

        [Test]
        public async Task TestNpmMetadata()
        {
            var search = new SearchNpmPackage(NullLogger<SearchNpmPackage>.Instance);
            var assemblyLocation = typeof(SearchNpmPackage).Assembly.Location;
            var index = assemblyLocation.IndexOf(Path.DirectorySeparatorChar + "ServerTest" + Path.DirectorySeparatorChar);
            if (index == -1)
            {
                return; // Inconclusive test
            }
            var checkoutDir = assemblyLocation.Substring(0, index);
            var packagesDir = Path.Combine(checkoutDir, "ServerAndAngular", "ClientApp");
            var package = await search.SearchMetadataAsync("@angular/core", "11.0.4", new SearchPackageConfiguration(packagesDir));
            Assert.AreEqual("@angular/core", package.PackageId);
            Assert.AreEqual("11.0.4", package.Version);
            Assert.AreEqual("MIT", package.License);
            Assert.AreEqual(PackageType.Npm, package.PackageType);
            Assert.AreEqual("Angular - the core framework", package.Description);
        }
    }
}
