using System;
using System.Collections.Generic;
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
        public async Task<ActionResult<IEnumerable<SOUPSearchProject>>> GetProjects()
        {
            // Return only project, without found SOUP
            return await _context.Projects.ToListAsync();
        }

        // GET: api/Projects/5
        [HttpGet("{projectId}")]
        public async Task<ActionResult<ProjectDto>> GetProject(string projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            _context.Entry(project);
            if (project == null)
            {
                return NotFound();
            }

            return project.ToDto();
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{projectId}")]
        public async Task<IActionResult> PutProject(string projectId, SOUPSearchProject project)
        {
            if (projectId != project.Name)
            {
                return BadRequest();
            }

            _context.Entry(project).State = EntityState.Modified;

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
        public async Task<ActionResult<SOUPSearchProject>> StartProject(string projectId)
        {
            // Retrieve the project to start
            var project = await _context.Projects.FindAsync(projectId);
            _context.Entry(project).Reference(r => r.Repository).Load();
            switch (project.Repository)
            {
                case GitRepository git :
                    _context.Entry((GitRepository)project.Repository).Reference(r => r.SshKey).Load();
                break;
                default: throw new ApplicationException($"The repository Type {project.Repository?.GetType()} is not supported!");
            }
            // Create the job to process the project
            var projectJob = _serviceProvider.GetService<IProjectJob>();
            projectJob.Project = project;
            // Add the Job to the JobManager
            var task = _projectJobManager.StartTask(projectJob);
            if (task == null)
            {
                return Problem($"The project {project.Name} is already processing !");
            }

            return CreatedAtAction("Process Project", new { id = projectId }, project);
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
            _context.Entry(project).Collection(p => p.PackageConsumers).Load();
            _context.PackageConsumer.RemoveRange(project.PackageConsumers);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return project;
        }

        private bool ProjectExists(string projectId)
        {
            return _context.Projects.Any(e => e.Name == projectId);
        }
    }
}
