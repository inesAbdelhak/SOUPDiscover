using SoupDiscover.ORM;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoupDiscover.Core.Respository
{
    public abstract class Repository : EntityObject
    {
        [Required(AllowEmptyStrings = false)]
        [Key]
        public string Name { get; set; }
    }
}
