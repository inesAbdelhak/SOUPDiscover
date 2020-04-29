using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace testAngulardotnet.ORM
{
    /// <summary>
    /// 
    /// </summary>
    public class Project
    {
        /// <summary>
        /// The primary  key of the project
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The repository where search files
        /// </summary>
        public Repository Repository { get; set; }

        /// <summary>
        /// All packages found for this repository
        /// </summary>
        public ICollection<Package> Packages { get; set; }
    }
}
