using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoupDiscover.Common;
using SoupDiscover.Dto;
using SoupDiscover.ICore;
using SoupDiscover.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private string CheckProject(ProjectDto project, DataContext context)
        {
            var errors = string.Empty;
            if (project == null)
            {
                return "Not project fields. ";
            }
            if (string.IsNullOrEmpty(project.Name))
            {
                return $"The {nameof(project.Name)} of project is not defined. ";
            }
            if (string.IsNullOrEmpty(project.NugetServerUrl))
            {
                return $"The {nameof(project.NugetServerUrl)} of project is not defined. ";
            }            
            if (string.IsNullOrEmpty(project.RepositoryId))
            {
                return $"The {nameof(project.RepositoryId)} of project is not defined. ";
            }
            else
            {
                // Check if repository id given exists
                var repository = context.Repositories.Find(project.RepositoryId);
                if (repository == null)
                {
                    errors += $"The repositoryid {project.RepositoryId} not found in database. ";
                }
            }
            if (errors == string.Empty)
            {
                return null;
            }
            return errors;
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
            var errors = CheckProject(project, _context);
            if (errors != null)
            {
                return Problem(errors);
            }

            if (_projectJobManager.IsRunning(projectId))
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
        public async Task<ActionResult<ProjectEntity>> PostProject(ProjectDto project)
        {
            var errors = CheckProject(project, _context);
            if (errors != null)
            {
                return Problem(errors);
            }
            var projectEntity = project.ToModel();
            _context.Projects.Add(projectEntity);
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
            if (project == null )
            {
                return Problem($"The project id {projectId} is not found.");
            }
            _context.Entry(project).Reference(r => r.Repository).Load();
            switch (project.Repository)
            {
                case GitRepository git:
                    _context.Entry((GitRepository)project.Repository).Reference(r => r.Credential).Load();
                    break;
                default: throw new SoupDiscoverException($"The repository Type {project.Repository?.GetType()} is not supported!");
            }
            // Create the job to process the project
            var projectJob = _serviceProvider.GetRequiredService<IProjectJob>();
            projectJob.SetProject(project.ToDto(), _serviceProvider);

            // Add the Job to the JobManager
            if (_projectJobManager.ExecuteTask(projectJob) == null)
            {
                return Problem($"The project {project.Name} is already processing !");
            }

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
        public async Task<ActionResult<ProjectEntity>> DeleteProject(string projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }
            if (_projectJobManager.IsRunning(projectId))
            {
                return Problem($"The project {projectId} is running. Stop it before remove it.");
            }

            _context.Entry(project).Collection(p => p.PackageConsumers).Load();
            _context.PackageConsumer.RemoveRange(project.PackageConsumers);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            project.PackageConsumers = null; // Fix json cycle serialization
            return project;
        }

        /// <summary>
        /// Get all project consumer of a project (all csproj of a project)
        /// </summary>
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
