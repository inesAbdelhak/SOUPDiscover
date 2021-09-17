using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoupDiscover.ORM
{
    public class PackageConsumer : EntityObject
    {
        [Key]
        public int PackageConsumerId { get; set; }

        public virtual ProjectEntity Project { get; set; }

        /// <summary>
        /// The name of the project associated
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// The name of the package Consumer (the csproj file)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All package used by this package consumer
        /// </summary>
        public virtual ICollection<PackageConsumerPackage> Packages { get; set; }
    }
}
