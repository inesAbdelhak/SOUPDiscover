using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// A nuget package or an npm package
    /// </summary>
    public class Package : EntityObject
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
        /// The license can be an Url, an expression or a file.
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// The license type, <see cref="LicenseType"/> for more info.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LicenseType LicenseType { get; set; }

        /// <summary>
        /// The Url to the package
        /// </summary>
        public string ProjectUrl { get; set; }

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

        /// <summary>
        /// The type of repository where is source code.
        /// Ex : git
        /// </summary>
        public string RepositoryType { get; set; }
        
        /// <summary>
        /// The url where is source code
        /// </summary>
        public string RepositoryUrl { get; set; }

        public string RepositoryCommit { get; set; }

        /// <summary>
        /// Package vulnerabilities meta data.
        /// </summary>
        public IList<VulnerabilityMetaData> Vulnerabilities { get; set; }



    }
}
