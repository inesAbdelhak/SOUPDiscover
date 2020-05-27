using log4net.Core;
using Microsoft.Extensions.Logging;
using SoupDiscover.ORM;
using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace SoupDiscover.Common
{
    /// <summary>
    /// Search nuget package meta-data from HTTP source server
    /// </summary>
    public class SearchNugetPackageMetada : ISearchNugetPackageMetada
    {
        private Lazy<WebClient> _webClient = new Lazy<WebClient>(() => new WebClient());
        private readonly ILogger<SearchNugetPackageMetada> _logger;

        public SearchNugetPackageMetada(ILogger<SearchNugetPackageMetada> logger)
        {
            _logger = logger;
        }

        public Package SearchMetadata(string packageId, string version, string[] sources)
        {
            if(sources == null || sources.Length == 0)
            {
                return new Package() { PackageId = packageId, Version = version, Licence = null, PackageType = PackageType.Nuget };
            }
            // Retrieve package source in
            string xml = null;
            foreach (var source in sources)
            {
                try
                {
                    xml = _webClient.Value.DownloadString($"{source}/Packages(Id='{packageId}',Version='{version}')");
                }
                catch (Exception)
                {
                }
                if(xml != null)
                {
                    break;
                }
            }
            string licenceUrl = null;
            string description = null;
            if (xml != null)
            {
                var document = XDocument.Parse(xml);
                var properties = document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "properties");
                licenceUrl = properties?.Elements().FirstOrDefault(e => e.Name.LocalName == "LicenseUrl").Value;
                description = properties?.Elements().FirstOrDefault(e => e.Name.LocalName == "Description").Value;
            }
            else
            {
                _logger.LogInformation($"Unable to find Meta-data on package {packageId} version {version} on sources {string.Join(";", sources)}");
            }
            return new Package() { PackageId = packageId, Version = version, Licence = licenceUrl, PackageType = PackageType.Nuget };
        }
    }
}
