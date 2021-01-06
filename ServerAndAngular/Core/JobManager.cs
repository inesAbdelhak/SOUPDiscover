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
    public class JobManager : IJobManager
    {
        /// <summary>
        /// The list of processing projects
        /// object : id of the job
        /// </summary>
        private readonly Dictionary<object, ExecutingTask> _processingJobs = new Dictionary<object, ExecutingTask>();
        private Queue<ExecutingTask> _waitingJobs = new Queue<ExecutingTask>();

        /// <summary>
        /// The object that permit to sync tasks to <see cref="_processingJobs"/>
        /// </summary>
        private readonly object _syncObject = new object();

        protected IServiceProvider _serviceProvider;
        protected ILogger _logger;
        protected int _maxJobInParallel;

        protected JobManager(ILogger logger, int maxJobInParallel)
        {
            _logger = logger;
            _maxJobInParallel = maxJobInParallel;
        }

        /// <summary>
        /// Launch the processing of the project
        /// </summary>
        /// <param name="project">The project to process</param>
        /// <returns>false : The project is already processing else true</returns>
        public Task<TJob> ExecuteTask<TJob>(TJob job)
            where TJob : IJob
        {
            var executingTask = new ExecutingTask();

            Task task = null;
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
                executingTask.CancellationTokenSource = tokenSource;
                _logger.LogInformation($"Start the Job {job.IdJob}");
                task = new Task(() => job.ExecuteAsync(executingTask.CancellationTokenSource.Token).Wait());
                finalTask = task.ContinueWith(t => EndProcessingJob(job));
                executingTask.Task = task;
                if (_processingJobs.Count >= _maxJobInParallel)
                {
                    // Start process later
                    _waitingJobs.Enqueue(executingTask);
                }
                else
                {
                    // Start process now
                    _processingJobs.Add(job.IdJob, executingTask);
                    executingTask.Task.Start();
                }
            }
            return finalTask;
        }

        /// <summary>
        /// Call at the end of processing the project
        /// </summary>
        /// <param name="initialTask">the task that processing the</param>
        /// <param name="job">The object, that manage the processing of the project</param>
        private TJob EndProcessingJob<TJob>(TJob job) where TJob : IJob
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

                _waitingJobs.TryDequeue(out var nextTask);
                if (nextTask != null)
                {
                    _processingJobs.Add(nextTask.Job.IdJob, nextTask);
                    nextTask.Task.Start();
                }
            }
            return job;
        }

        /// <summary>
        /// Return the list of processing or waiting to process project id
        /// </summary>
        public object[] GetProcessingJobIds()
        {
            lock (_syncObject)
            {
                return _processingJobs.Keys.Concat(_waitingJobs.Select(t => t.Job.IdJob)).ToArray();
            }
        }

        /// <summary>
        /// Return the list of processing project id
        /// </summary>
        public ExecutingTask[] GetProcessingJob()
        {
            lock (_syncObject)
            {
                return _processingJobs.Values.Concat(_waitingJobs).ToArray();
            }
        }

        /// <summary>
        /// Cancel the executing project
        /// </summary>
        /// <param name="jobId">Id of the job to stop</param>
        public bool Cancel(string jobId)
        {
            lock (_syncObject)
            {
                if (_processingJobs.TryGetValue(jobId, out ExecutingTask executingTask))
                {
                    executingTask.CancellationTokenSource.Cancel();
                    return true;
                }

                executingTask = _waitingJobs.FirstOrDefault(e => (string)e.Job.IdJob == jobId);
                if (executingTask != null)
                {
                    var newQueue = _waitingJobs.ToList();
                    newQueue.Remove(executingTask);
                    _waitingJobs = new Queue<ExecutingTask>(newQueue);
                    return true;
                }
            }
            return false;
        }

        public bool IsRunning(string jobId)
        {
            lock (_syncObject)
            {
                return _processingJobs.ContainsKey(jobId) || _waitingJobs.Any(e => (string)e.Job.IdJob == jobId);
            }
        }
    }
}
