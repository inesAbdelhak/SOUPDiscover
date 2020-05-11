using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// A repository to process or processed
    /// </summary>
    public class SOUPSearchProject
    {
        private static readonly char delimiter = ';';

        private string _sOUPTypeToSearchCollection;
        
        /// <summary>
        /// The primary  key of the project
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The repository where search files
        /// </summary>
        [Required]
        public virtual Repository Repository { get; set; }

        /// <summary>
        /// The foreign key to the repository definition
        /// </summary>
        [Required]
        public int RepositoryId { get; set; }

        /// <summary>
        /// All packages found for this repository
        /// </summary>
        public virtual ICollection<Package> Packages { get; set; }

        /// <summary>
        /// The current status of the project
        /// </summary>
        public ProcessStatus ProcessStatus { get; set; }

        /// <summary>
        /// The command lines to execute before search and parse files
        /// </summary>
        public string CommandLinesBeforeParse { get; set; }

        /// <summary>
        /// List of Url nuget server where find metadata of packages
        /// </summary>
        public string NugetServerUrl { get; set; }

        [NotMapped]
        public SOUPToSearch[] SOUPTypeToSearch
        {
            get { return _sOUPTypeToSearchCollection.Split(delimiter).Select(e => Enum.Parse<SOUPToSearch>(e)).ToArray(); }
            set
            {
                _sOUPTypeToSearchCollection = string.Join($"{delimiter}", value);
            }
        }
    }
}
