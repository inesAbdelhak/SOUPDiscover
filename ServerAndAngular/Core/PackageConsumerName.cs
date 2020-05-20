using System.Collections.Generic;

namespace SoupDiscover.Common
{
    public class PackageConsumerName
    {

        public PackageConsumerName(string name, PackageName[] packages)
        {
            Name = name;
            Packages = packages;
        }

        public string Name { get; }

        public PackageName[] Packages { get; }
    }
}
