using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoupDiscover.Core
{
    /// <summary>
    /// Represent a job manager.
    /// A jobManager manage a collection of <see cref="IJOb"/>.
    /// </summary>
    public abstract class JobManager : IJobManager
    {
        /// <summary>
        /// The list of processing projects
        /// </summary>
        private readonly Dictionary<object, ExecutingTask> _processingJobs = new Dictionary<object, ExecutingTask>();

        /// <summary>
        /// The object that permit to sync tasks to <see cref="_processingJobs"/>
        /// </summary>
        private readonly object _syncObject = new object();

        protected IServiceProvider _serviceProvider;
        protected ILogger _logger;

        protected JobManager(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Launch the processing of the project
        /// </summary>
        /// <param name="project">The project to process</param>
        /// <returns>false : The project is already processing else true</returns>
        public Task<TJob> StartTask<TJob>(TJob job)
            where TJob : IJob
        {
            var task = new ExecutingTask();
            // Check if this project is not currently processing
            lock (_syncObject)
            {
                if (_processingJobs.ContainsKey(job.IdJob))
                {
                    _logger.LogDebug($"Try to start a job that is already running");
                    return null; // Already processing
                }
                // Create a task to start the processing
                var tokenSource = new CancellationTokenSource();
                task.CancellationTokenSource = tokenSource;
                _logger.LogInformation($"Start the Job {job.IdJob}");
                var finalTask = job.StartAsync(task.CancellationTokenSource.Token)
                    .ContinueWith(t => EndProcessingJob(job), task.CancellationTokenSource.Token)
                    .ContinueWith(t => job, task.CancellationTokenSource.Token);
                _processingJobs.Add(job.IdJob, task);
                task.Task = finalTask;
                return finalTask;
            }
        }

        /// <summary>
        /// Call at the end of processing the project
        /// </summary>
        /// <param name="initialTask">the task that processing the</param>
        /// <param name="job">The object, that manage the processing of the project</param>
        private void EndProcessingJob(IJob job)
        {
            if (job == null)
            {
                throw new ApplicationException($"The parameter {nameof(job)} must be not null!");
            }
            var jobCasted = job as IJob;
            if (jobCasted == null)
            {
                throw new ApplicationException($"The parameter {nameof(job)} must be type of {typeof(IJob)}!");
            }
            lock (_syncObject)
            {
                if (!_processingJobs.ContainsKey(jobCasted.IdJob))
                {
                    throw new ApplicationException($"Unable to find the Task for the project Id {jobCasted.IdJob}");
                }
                // Remove the task from processing tasks
                _processingJobs.Remove(jobCasted.IdJob);
            }
        }

        /// <summary>
        /// Return the list of processing project id
        /// </summary>
        public object[] GetProcessingJobIds()
        {
            lock (_syncObject)
            {
                return _processingJobs.Keys.ToArray();
            }
        }

        /// <summary>
        /// Return the list of processing project id
        /// </summary>
        public ExecutingTask[] GetProcessingJob()
        {
            lock (_syncObject)
            {
                return _processingJobs.Values.ToArray();
            }
        }
    }
}
