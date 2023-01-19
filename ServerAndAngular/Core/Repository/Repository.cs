using System.ComponentModel.DataAnnotations;
using SoupDiscover.ORM;

namespace SoupDiscover.Core.Repository
{
    public abstract class Repository : EntityObject
    {
        [Required(AllowEmptyStrings = false)]
        [Key]
        public string Name { get; set; }
    }
}
