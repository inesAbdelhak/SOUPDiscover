using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 

namespace SoupDiscover.ORM
{
    public class AdvisoryIdentifier : EntityObject
    {
        [Key]
        public int Id { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public int VulnerabilityId { get; set; }
        [ForeignKey("VulnerabilityId")]
        public Vulnerability Vulnerability { get; set; }
    }
}
