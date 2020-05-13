using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoupDiscover.ORM
{
    public abstract class Repository : EntityObject
    {
        [Key]
        public string Name { get; set; }
    }
}
