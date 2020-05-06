using SoupDiscover.ORM;

namespace SoupDiscover.Core
{
    /// <summary>
    /// A job to process a project
    /// </summary>
    public interface IProjectJob : IJob
    {
        /// <summary>
        /// The project to process
        /// </summary>
        Project Project { get; set; }
    }
}
