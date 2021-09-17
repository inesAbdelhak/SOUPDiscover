using SoupDiscover.Core.Respository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoupDiscover.ORM
{
    /// <summary>
    /// A repository to process or processed
    /// </summary>
    public class ProjectEntity : EntityObject
    {
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
        /// The packages associated with a package consumer (package used by a project)
        /// </summary>
        public virtual ICollection<PackageConsumer> PackageConsumers { get; set; }

        /// <summary>
        /// The current status of the project
        /// </summary>
        [NotMapped]
        public ProcessStatus ProcessStatus { get; set; }

        public ProcessStatus GetProcessStatus()
        {
            if (string.IsNullOrEmpty(LastAnalysisError))
            {
                return ProcessStatus.Waiting;
            }
            return ProcessStatus.Error;
        }

        /// <summary>
        /// The command lines to execute before search and parse files
        /// </summary>
        public string CommandLinesBeforeParse { get; set; }

        /// <summary>
        /// List of Url nuget server where find metadata of packages
        /// </summary>
        public string NugetServerUrl { get; set; }

        /// <summary>
        /// The last analysis error
        /// </summary>
        public string LastAnalysisError { get; set; }

        public DateTime? LastAnalysisDate { get; set; }
    }
}
