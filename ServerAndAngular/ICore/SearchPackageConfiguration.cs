using SoupDiscover.ORM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoupDiscover.Common
{
    /// <summary>
    /// Parameters to use, to search package metadata
    /// </summary>
    public class SearchPackageConfiguration
    {
        private IDictionary<PackageType, string[]> _sources;

        public SearchPackageConfiguration(string checkoutDirectory, IDictionary<PackageType, string[]> sources = null)
        {
            CheckoutDirectory = checkoutDirectory;
            if (sources == null)
            {
                _sources = new Dictionary<PackageType, string[]>();
            }
            else
            {
                _sources = sources;
            }
        }

        /// <summary>
        /// The directory where the repository is checkout
        /// </summary>
        public string CheckoutDirectory { get; }

        /// <summary>
        /// Add sources for a type of package
        /// </summary>
        public void AddSources(PackageType packageType, string[] sources)
        {
            if (sources == null)
            {
                return;
            }
            if (_sources.ContainsKey(packageType))
            {
                _sources[packageType] = _sources[packageType].Concat(sources).Distinct().ToArray();
                return;
            }
            _sources.Add(packageType, sources.Distinct().ToArray());
        }

        /// <summary>
        /// Get sources where search metadata for a type of packages 
        /// </summary>
        /// <param name="packageType"></param>
        /// <returns></returns>
        public string[] GetSources(PackageType packageType)
        {
            if (_sources.TryGetValue(packageType, out var sources))
            {
                return sources;
            }
            return null;
        }
    }
}
