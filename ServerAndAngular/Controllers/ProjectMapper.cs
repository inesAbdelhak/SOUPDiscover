using SoupDiscover.ORM;
using System.Collections.Generic;

namespace SoupDiscover.Controllers
{
    /// <summary>
    /// Mapping between <see cref="SOUPSearchProject"/> and <see cref="ProjectDto"/>
    /// </summary>
    public static class ProjectMapper
    {
        /// <summary>
        /// Convert a <see cref="SOUPSearchProject"/> to a <see cref="ProjectDto"/> 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static ProjectDto ToDto(this SOUPSearchProject project)
        {
            if (project == null)
            {
                return null;
            }
            return new ProjectDto()
            {
                CommandLinesBeforeParse = project.CommandLinesBeforeParse,
                Name = project.Name,
                NugetServerUrl = project.NugetServerUrl,
                ProcessStatus = project.ProcessStatus,
                Repository = project.Repository.ToDto(),
                RepositoryId = project.RepositoryId,
                LastAnalysisError = project.LastAnalysisError,
            };
        }

        /// <summary>
        /// Convert a <see cref="SOUPSearchProject"/> to a <see cref="ProjectDto"/> 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectDto> ToDto(this IEnumerable<SOUPSearchProject> project)
        {
            if (project == null)
            {
                yield break;
            }
            foreach (var p in project)
            {
                yield return p.ToDto();
            }
        }

        /// <summary>
        /// Convert a <see cref="ProjectDto"/> to a <see cref="SOUPSearchProject"/>
        /// </summary>
        public static SOUPSearchProject ToModel(this ProjectDto projectDto)
        {
            if (projectDto == null)
            {
                return null;
            }
            return new SOUPSearchProject()
            {
                CommandLinesBeforeParse = projectDto.CommandLinesBeforeParse,
                Name = projectDto.Name,
                NugetServerUrl = projectDto.NugetServerUrl,
                ProcessStatus = projectDto.ProcessStatus,
                Repository = projectDto.Repository.ToModel(),
                RepositoryId = projectDto.RepositoryId,
                LastAnalysisError = projectDto.LastAnalysisError,
            };
        }
    }
}
