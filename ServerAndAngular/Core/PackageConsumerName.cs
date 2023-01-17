namespace SoupDiscover.Core
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
