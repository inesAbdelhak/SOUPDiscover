using System.ComponentModel.DataAnnotations.Schema;

namespace SoupDiscover.ORM
{
    public class Repository : EntityObject
    {
        public int ID { get; set; }
         
        /// <summary>
        /// The name of the repository to display
        /// </summary>
        public string Name { get; set; }
    }
}
