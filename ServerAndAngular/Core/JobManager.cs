using Microsoft.Extensions.Logging;
using SoupDiscover.ICore;
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
        public Task<TJob> ExecuteTask<TJob>(TJob job)
            where TJob : IJob
        {
            var task = new ExecutingTask();
           
            Task<TJob> finalTask = null;
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
                finalTask = Task.Run(() => job.ExecuteAsync(task.CancellationTokenSource.Token).Wait())
                    .ContinueWith(t => EndProcessingJob(job));
                _processingJobs.Add(job.IdJob, task);
                task.Task = finalTask;
            }
            return finalTask;
        }

        /// <summary>
        /// Call at the end of processing the project
        /// </summary>
        /// <param name="initialTask">the task that processing the</param>
        /// <param name="job">The object, that manage the processing of the project</param>
        private TJob EndProcessingJob<TJob>(TJob job) where TJob: IJob
        {
            if (job == null)
            {
                throw new ApplicationException($"The parameter {nameof(job)} must be not null!");
            }
            lock (_syncObject)
            {
                if (!_processingJobs.ContainsKey(job.IdJob))
                {
                    throw new ApplicationException($"Unable to find the Task for the project Id {job.IdJob}");
                }
                // Remove the task from processing tasks
                _processingJobs.Remove(job.IdJob);
            }
            return job;
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

        /// <summary>
        /// Cancel the executing project
        /// </summary>
        /// <param name="jobId">Id of the job to stop</param>
        public bool Cancel(string jobId)
        {
            ExecutingTask executingTask;
            lock (_syncObject)
            {
                if (!_processingJobs.TryGetValue(jobId, out executingTask))
                {
                    return false;
                }
            }
            executingTask.CancellationTokenSource.Cancel();
            return true;
        }

        public bool IsRunning(string jobId)
        {
            lock(_syncObject)
            {
                return _processingJobs.ContainsKey(jobId);
            }
        }
    }
}
