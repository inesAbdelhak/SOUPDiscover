using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;
using SoupDiscover.Core;
using SoupDiscover.ORM;

namespace SoupDiscover.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IProjectJobManager _projectJobManager;
        private readonly IServiceProvider _serviceProvider;

        public ProjectsController(DataContext context, IProjectJobManager projectJobManager, IServiceProvider serviceProvider)
        {
            _context = context;
            _projectJobManager = projectJobManager;
            _serviceProvider = serviceProvider;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            // Return only project, without found SOUP
            var projectsTask = _context.Projects.ToListAsync();
            var processingProjects = _projectJobManager.GetProcessingJobIds();
            var projects = await projectsTask;
            projects.ForEach(e => e.ProcessStatus = processingProjects.Contains(e.Name) ? ProcessStatus.Running : e.GetProcessStatus());
            return Ok(projects.ToDto());
        }

        // GET: api/Projects/5
        [HttpGet("{projectId}")]
        public async Task<ActionResult<ProjectDto>> GetProject(string projectId)
        {
            var projectTask = _context.Projects.FindAsync(projectId);
            var isRunning = _projectJobManager.IsRunning(projectId);
            var project = await projectTask;
            _context.Entry(project);
            if (project == null)
            {
                return NotFound();
            }
            // The status is not on database
            project.ProcessStatus = isRunning ? ProcessStatus.Running : project.GetProcessStatus();
            return project.ToDto();
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{projectId}")]
        public async Task<IActionResult> PutProject(string projectId, ProjectDto project)
        {
            if (projectId != project.Name)
            {
                return BadRequest();
            }

            if(_projectJobManager.IsRunning(projectId))
            {
                return Problem($"The project {projectId} is running. Stop the project before update it.");
            }

            var projectModel = _context.Projects.Find(project.Name);
            _context.Projects.Update(projectModel);
            projectModel.NugetServerUrl = project.NugetServerUrl;
            projectModel.RepositoryId = project.RepositoryId;
            projectModel.CommandLinesBeforeParse = project.CommandLinesBeforeParse;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(projectId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Projects
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<SOUPSearchProject>> PostProject(SOUPSearchProject project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.Name }, project);
        }

        // POST: api/Projects/Start/{id}
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Start the processing of the project
        /// </summary>
        /// <param name="projectId">Id of the project to Start</param>
        /// <returns></returns>
        [HttpPost("Start/{projectId}")]
        public ActionResult<bool> StartProject(string projectId)
        {
            // Retrieve the project to start
            var project = _context.Projects.Find(projectId);
            _context.Entry(project).Reference(r => r.Repository).Load();
            switch (project.Repository)
            {
                case GitRepository git:
                    _context.Entry((GitRepository)project.Repository).Reference(r => r.SshKey).Load();
                    break;
                default: throw new ApplicationException($"The repository Type {project.Repository?.GetType()} is not supported!");
            }
            // Create the job to process the project
            var projectJob = _serviceProvider.GetRequiredService<IProjectJob>();
            projectJob.SetProject(project.ToDto(), _serviceProvider);

            // Add the Job to the JobManager
            if (_projectJobManager.GetProcessingJobIds().Contains(projectId))
            {
                return Problem($"The project {project.Name} is already processing !");
            }
            _projectJobManager.StartTask(projectJob);

            return CreatedAtAction("Process Project", new { id = projectId }, true);
        }

        /// <summary>
        /// Stop the executing project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpPost("Stop/{projectId}")]
        public ActionResult<bool> StopProject(string projectId)
        {
            var isCanceled = _projectJobManager.Cancel(projectId);
            return CreatedAtAction("Stop process Project", new { id = projectId }, isCanceled);
        }

        // DELETE: api/Projects/5
        [HttpDelete("{projectId}")]
        public async Task<ActionResult<SOUPSearchProject>> DeleteProject(string projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }
            if(_projectJobManager.IsRunning(projectId))
            {
                return Problem($"The project {projectId} is running. Stop it before remove it.");
            }

            _context.Entry(project).Collection(p => p.PackageConsumers).Load();
            _context.PackageConsumer.RemoveRange(project.PackageConsumers);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return project;
        }

        /// <summary>
        /// Get all project consumer of a project (all csproj of a project)
        /// </summary>
        /// <returns></returns>
        [HttpGet("projectConsumers/{projectName}")]
        public async Task<ActionResult<string[]>> GetProjectConsumers(string projectName)
        {
            return _context.PackageConsumer.Where(p => p.ProjectId == projectName).Select(p => p.Name).Distinct().ToArray();
        }

        private bool ProjectExists(string projectId)
        {
            return _context.Projects.Any(e => e.Name == projectId);
        }
    }
}
