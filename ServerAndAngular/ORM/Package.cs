using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// A nuget package or an npm package
    /// </summary>
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
        /// For npm package, the type of license
        /// </summary>
        public string Licence { get; set; }

        /// <summary>
        /// The Url to the package
        /// </summary>
        public string PackageUrl { get; set; }
        
        /// <summary>
        /// Nuget package or npm package
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PackageType PackageType { get; set; }

        /// <summary>
        /// All package consumer to use this package
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<PackageConsumer> PackageConsumers { get; set; }
        
        /// <summary>
        /// Description found in metadata
        /// </summary>
        public string Description { get; set; }
    }
}
