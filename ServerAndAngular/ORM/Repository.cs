using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoupDiscover.ORM
{
    public abstract class Repository : EntityObject
    {
        public int ID { get; set; }
         
        /// <summary>
        /// The name of the repository to display
        /// </summary>
        [Required]
        public string Name { get; set; }
    }
}
