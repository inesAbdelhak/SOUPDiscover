using Microsoft.CodeAnalysis.CSharp.Syntax;
using SoupDiscover.Common;
using SoupDiscover.Core.Respository;
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
        private string _sOUPTypeToSearchCollection;
        
        /// <summary>
        /// The primary  key of the project
        /// </summary>
        [Key]
        public string Name { get; set; }

        /// <summary>
        /// The repository where search files
        /// </summary>
        public virtual Repository Repository { get; set; }

        /// <summary>
        /// The foreign key to the repository definition
        /// </summary>
        [Required]
        public string RepositoryId { get; set; }

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
        public PackageType[] SOUPTypeToSearch
        {
            get
            {
                return EnumExtension.Deserialize<PackageType>(_sOUPTypeToSearchCollection).ToArray();
            }
            set
            {
                _sOUPTypeToSearchCollection = value.Serialize();
            }
        }
    }
}
