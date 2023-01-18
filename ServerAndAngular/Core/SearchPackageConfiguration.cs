using System.Collections.Generic;
using SoupDiscover.ORM;

namespace SoupDiscover.Core
{
    /// <summary>
    /// Parameters to use, to search package metadata
    /// </summary>
    public class SearchPackageConfiguration
    {
        private readonly IDictionary<PackageType, HashSet<string>> _sources;

        public SearchPackageConfiguration(string checkoutDirectory, IDictionary<PackageType, HashSet<string>> sources = null)
        {
            CheckoutDirectory = checkoutDirectory;
            _sources = sources ?? new Dictionary<PackageType, HashSet<string>>();
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

            if (!_sources.ContainsKey(packageType))
            {
                _sources.Add(packageType, new HashSet<string>());
            }

            foreach (var source in sources)
            {
                _sources[packageType].Add(source);
            }
        }

        /// <summary>
        /// Get sources where search metadata for a type of packages 
        /// </summary>
        /// <param name="packageType"></param>
        /// <returns></returns>
        public HashSet<string> GetSources(PackageType packageType)
        {
            return _sources.TryGetValue(packageType, out var sources) ? sources : null;
        }
    }
}
