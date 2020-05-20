using SoupDiscover.Common;
using SoupDiscover.ORM;
using System;

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
        SOUPSearchProject Project { get; set; }
    }
}
