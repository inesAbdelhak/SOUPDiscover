using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SoupDiscover.Common;
using SoupDiscover.Controllers;
using SoupDiscover.Core;
using SoupDiscover.Core.Respository;
using SoupDiscover.ORM;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ServerTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestsshConfigFiles()
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
        public void TestNugetMetadata()
        {
            var search = new SearchNugetPackage(NullLogger<SearchNugetPackage>.Instance);
            var package = search.SearchMetadata("log4net", "2.0.8", new[] { @"https://www.nuget.org/api/v2" });
            Assert.AreEqual("log4net", package.PackageId);
            Assert.AreEqual("2.0.8", package.Version);
            Assert.AreEqual("http://logging.apache.org/log4net/license.html", package.Licence);
            Assert.IsTrue(package.Description.StartsWith("log4net is a tool to help the programmer output "));
        }

        [Test]
        public void TestNpmMetadata()
        {
            var search = new SearchNpmPackage(NullLogger<SearchNpmPackage>.Instance);
            var assemblyLocation = typeof(SearchNpmPackage).Assembly.Location;
            var index = assemblyLocation.IndexOf(Path.DirectorySeparatorChar + "ServerTest" + Path.DirectorySeparatorChar);
            if(index == -1)
            {
                return; // Inconclusive test
            }
            var checkoutDir = assemblyLocation.Substring(0, index);
            var packagesDir = Path.Combine(checkoutDir, "ServerAndAngular", "ClientApp");
            var package = search.SearchMetadata("@angular/core", "8.2.12", packagesDir);
            Assert.AreEqual("@angular/core", package.PackageId);
            Assert.AreEqual("8.2.12", package.Version);
            Assert.AreEqual("MIT", package.Licence);
            Assert.AreEqual(PackageType.Npm, package.PackageType);
            Assert.AreEqual("Angular - the core framework", package.Description);
        }
    }
}
