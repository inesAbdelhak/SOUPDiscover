using SoupDiscover.Dto;
using System;

namespace SoupDiscover.ICore
{
    /// <summary>
    /// A job to process a project
    /// </summary>
    public interface IProjectJob : IJob
    {
        /// <summary>
        /// The project to process
        /// </summary>
        ProjectDto ProjectDto { get; }

        void SetProject(ProjectDto project, IServiceProvider provider);
    }
}
