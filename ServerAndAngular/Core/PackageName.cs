using SoupDiscover.ORM;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SoupDiscover.Common
{
    public class PackageName : IEquatable<PackageName>
    {
        public PackageName(string id, string version, PackageType packageType)
        {
            PackageId = id;
            Version = version;
            PackageType = packageType;
        }

        public string GetSerialized()
        {
            return $"{PackageId}/{Version}";
        }

        public string PackageId { get; }

        public string Version { get; }

        public PackageType PackageType { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is PackageName))
            {
                return false;
            }
            return Equals((PackageName)obj);
        }

        public bool Equals([AllowNull] PackageName other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return PackageId == other.PackageId && Version == other.Version;
        }
        public override int GetHashCode()
        {
            return PackageId.GetHashCode() ^ Version.GetHashCode();
        }
    }
}
