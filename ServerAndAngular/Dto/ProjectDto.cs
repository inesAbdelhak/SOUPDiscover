using SoupDiscover.ORM;
using System;
using System.Text.Json.Serialization;

namespace SoupDiscover.Dto
{
    public class ProjectDto
    {
        /// <summary>
        /// The primary  key of the project
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The repository where search files
        /// </summary>
        public virtual RepositoryDto Repository { get; set; }

        /// <summary>
        /// The foreign key to the repository definition
        /// </summary>
        public string RepositoryId { get; set; }

        /// <summary>
        /// The current status of the project
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProcessStatus ProcessStatus { get; set; }

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
