using System.ComponentModel.DataAnnotations;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// The many to many relation between PackageConsumer and Package
    /// </summary>
    public class PackageConsumerPackage
    {
        [Key]
        public int Id { get; set; }
        
        public virtual PackageConsumer PackageConsumer { get; set; }

        public int PackageConsumerId { get; set; }

        public virtual Package Package { get; set; }

        public int PackageId { get; set; }
    }
}
