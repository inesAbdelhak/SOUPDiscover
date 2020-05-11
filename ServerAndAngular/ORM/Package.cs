using System.ComponentModel.DataAnnotations.Schema;

namespace SoupDiscover.ORM
{
    public class Package
    {
        /// <summary>
        /// Id of the package
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the package
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// Version of the package
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Nuget package or npm package
        /// </summary>
        public PackageType PackageType { get; set; }
    }
}
