using System.Threading.Tasks;

namespace SoupDiscover.Core
{
    /// <summary>
    /// Represent a job manager.
    /// A jobManager manage a collection of <see cref="IJOb"/>.
    /// </summary>
    public interface IJobManager
    {
        /// <summary>
        /// Add a job in the job manager
        /// </summary>
        /// <param name="job">The job to add</param>
        /// <returns>true : The job is added to the jobManager, false : the job is already running</returns>
        Task<TJob> StartTask<TJob>(TJob job) where TJob : IJob;

        ExecutingTask[] GetProcessingJob();

        object[] GetProcessingJobIds();
    }
}
