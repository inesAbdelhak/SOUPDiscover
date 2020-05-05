using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// A repository to process or processed
    /// </summary>
    public class Project
    {
        /// <summary>
        /// The primary  key of the project
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The repository where search files
        /// </summary>
        public virtual Repository Repository { get; set; }

        /// <summary>
        /// The forign key to the repository definition
        /// </summary>
        public int RepositoryId { get; set; }

        /// <summary>
        /// All packages found for this repository
        /// </summary>
        public virtual ICollection<Package> Packages { get; set; }

        /// <summary>
        /// The current status of the project
        /// </summary>
        public ProcessStatus ProcessStatus { get; set; }
    }
}
