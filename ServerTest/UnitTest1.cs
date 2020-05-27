using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SoupDiscover.Common;
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
            var search = new SearchNugetPackageMetada(NullLogger<SearchNugetPackageMetada>.Instance);
            var package = search.SearchMetadata("log4net", "2.0.8", new[] { @"https://www.nuget.org/api/v2" });
            Assert.AreEqual("log4net", package.PackageId);
            Assert.AreEqual("2.0.8", package.Version);
            Assert.AreEqual("http://logging.apache.org/log4net/license.html", package.Licence);
            Assert.IsTrue(package.Description.StartsWith("log4net is a tool to help the programmer output "));
        }

        [Test]
        public void TestNpmMetadata()
        {
            var search = new SearchNpmPackageMetadata(NullLogger<SearchNpmPackageMetadata>.Instance);
            var assemblyLocation = typeof(SearchNpmPackageMetadata).Assembly.Location;
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
        
        [Test]
        public void Test2()
        {
            File.Delete("CustomerDB.db");
            var services = new ServiceCollection();
            services.AddSingleton<IProjectJobManager, ProjectJobManager>();
            services.AddSingleton<ISearchNugetPackageMetada, FakeSearchNugetPackageMetada>();
            services.AddSingleton<ISearchNpmPackageMetadata, SearchNpmPackageMetadata>();
            services.AddTransient<IProjectJob, ProjectJob>();
            services.AddLogging();
            services.AddDbContext<DataContext>(optionBuilder => optionBuilder.UseSqlite(@"Data Source=CustomerDBTest.db;"));
            var provider = services.BuildServiceProvider();
            var job = provider.GetService<IProjectJob>();
            
            var context = provider.GetRequiredService<DataContext>();
            context.Database.EnsureCreated();

            Assert.IsNotNull(job);
            job.Project = new SOUPSearchProject()
            {
                Name="ProjetDeTest",
                CommandLinesBeforeParse = "dotnet restore --ignore-failed-sources\r\ncd ServerAndAngular\\ClientApp\r\nnpm i",
                ProcessStatus = ProcessStatus.Waiting,
                NugetServerUrl = @"https://www.nuget.org/api/v2",
                Repository = new GitRepository()
                {
                    Name = "NonoDSRepository",
                    Branch = "master",
                    SshKeyId = "testsshKey",
                    SshKey = new Credential()
                    {
                        key = GetSShPrivetKey(),
                        name = "testsshKey",
                    },
                    Url = "git@github.com:NonoDS/SOUPDiscover.git",
                },
            };
            context = provider.GetService<DataContext>();
            context.Projects.Add(job.Project);
            context.SaveChanges();
            job.Execute(CancellationToken.None);
        }

        public string GetSShPrivetKey()
        {
            return @"-----BEGIN OPENSSH PRIVATE KEY-----
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
-----END OPENSSH PRIVATE KEY-----
";
        }
    }
}
