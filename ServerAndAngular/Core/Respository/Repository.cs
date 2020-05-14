using SoupDiscover.ORM;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoupDiscover.Core.Respository
{
    public abstract class Repository : EntityObject
    {
        [Key]
        public string Name { get; set; }
    }
}
