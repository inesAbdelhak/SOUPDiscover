using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
            return await _context.Projects.ToListAsync();
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SOUPSearchProject>> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(string id, SOUPSearchProject project)
        {
            if (id != project.Name)
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
                if (!ProjectExists(id))
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
        [HttpPost("Start/{id}")]
        public async Task<ActionResult<SOUPSearchProject>> StartProject(int projectId)
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
            if (!_projectJobManager.StartTask(projectJob))
            {
                return Problem($"The project {project.Name} is already processing !");
            }

            return CreatedAtAction("Process Project", new { id = projectId }, project);
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<SOUPSearchProject>> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return project;
        }

        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.Name == id);
        }
    }
}
