using SoupDiscover.ORM;

namespace SoupDiscover.Controllers
{
    public static class ProjectMapper
    {
        /// <summary>
        /// Convert a <see cref="SOUPSearchProject"/> to a <see cref="ProjectDto"/> 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static ProjectDto ToDto(this SOUPSearchProject project)
        {
            if(project == null)
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
            };
        }

        /// <summary>
        /// Convert a <see cref="ProjectDto"/> to a <see cref="SOUPSearchProject"/>
        /// </summary>
        public static SOUPSearchProject ToModel(this ProjectDto projectDto)
        {
            if(projectDto == null)
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
            };
        }
    }
}
