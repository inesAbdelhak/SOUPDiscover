using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoupDiscover.ORM
{
    public class Package
    {
        /// <summary>
        /// Id of the package
        /// </summary>
        [Key]
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
        /// On nuget package, the url to the license
        /// </summary>
        public string Licence { get; set; }

        /// <summary>
        /// Nuget package or npm package
        /// </summary>
        public PackageType PackageType { get; set; }

        /// <summary>
        /// All package consumer to use this package
        /// </summary>
        public virtual ICollection<PackageConsumer> PackageConsumers { get; set; }

        /// <summary>
        /// All project that reference this package without PackageConsumer
        /// </summary>
        //public virtual ICollection<SOUPSearchProject> SOUPSearchProjects { get; set; }
    }
}
