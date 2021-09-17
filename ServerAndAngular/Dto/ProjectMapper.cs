using SoupDiscover.ORM;
using System.Collections.Generic;

namespace SoupDiscover.Dto
{

    /// <summary>
    /// Mapping between <see cref="ProjectEntity"/> and <see cref="ProjectDto"/>
    /// </summary>
    public static class ProjectMapper
    {
        /// <summary>
        /// Convert a <see cref="ProjectEntity"/> to a <see cref="ProjectDto"/> 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static ProjectDto ToDto(this ProjectEntity project)
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
                LastAnalysisDate = project.LastAnalysisDate,
            };
        }

        /// <summary>
        /// Convert a <see cref="ProjectEntity"/> to a <see cref="ProjectDto"/> 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static IEnumerable<ProjectDto> ToDto(this IEnumerable<ProjectEntity> project)
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
        /// Convert a <see cref="ProjectDto"/> to a <see cref="ProjectEntity"/>
        /// </summary>
        public static ProjectEntity ToModel(this ProjectDto projectDto)
        {
            if (projectDto == null)
            {
                return null;
            }
            return new ProjectEntity()
            {
                CommandLinesBeforeParse = projectDto.CommandLinesBeforeParse,
                Name = projectDto.Name,
                NugetServerUrl = projectDto.NugetServerUrl,
                ProcessStatus = projectDto.ProcessStatus,
                Repository = projectDto.Repository.ToModel(),
                RepositoryId = projectDto.RepositoryId,
                LastAnalysisError = projectDto.LastAnalysisError,
                LastAnalysisDate = projectDto.LastAnalysisDate,
            };
        }
    }
}
