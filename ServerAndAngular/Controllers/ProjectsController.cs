using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
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
            // Return only project, without found SOUP
            return await _context.Projects.ToListAsync();
        }

        // GET: api/Projects/5
        [HttpGet("{projectId}")]
        public async Task<ActionResult<SOUPSearchProject>> GetProject(string projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            _context.Entry(project).Collection(p => p.Packages).Load();
            if (project == null)
            {
                return NotFound();
            }

            return project;
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
#if DEBUG
            if (project.SOUPTypeToSearch == null || project.SOUPTypeToSearch.Length == 0)
            {
                project.SOUPTypeToSearch = new PackageType[] { PackageType.Nuget };
            }
#endif
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
        [HttpDelete("{projectId}")]
        public async Task<ActionResult<SOUPSearchProject>> DeleteProject(string projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return project;
        }

        private bool ProjectExists(string projectId)
        {
            return _context.Projects.Any(e => e.Name == projectId);
        }

        /// <summary>
        /// Export package into a csv file
        /// </summary>
        /// <param name="projectId">the project to export</param>
        /// <returns>A result with</returns>
        [HttpGet("exporttocsv/{projectId}")]
        public async Task<ActionResult> DownloadCsv(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return Problem($"Project Id not defined!");
            }
            var csvStream = ExportPackagesToCsvFile(projectId);
            if (csvStream == null)
            {
                return Problem($"Unable to find the project {projectId}");
            }
            return File(csvStream, "text/plain", $"{projectId}.csv");
        }

        /// <summary>
        /// Create a stream that contains the csv file
        /// </summary>
        /// <param name="projectId">Id of the project</param>
        private Stream ExportPackagesToCsvFile(string projectId, char delimiter = ';')
        {
            var project = _context.Projects.Find(projectId);
            if (project == null)
            {
                return null;
            }
            var baseStream = new MemoryStream();
            var stream = new StreamWriter(baseStream, System.Text.Encoding.UTF8);
            _context.Entry(project).Collection(p => p.Packages).Load();

            // Create header
            stream.WriteLine($"PackageId{delimiter}Version");
            foreach (var p in project.Packages)
            {
                stream.WriteLine($"{p.PackageId}{delimiter}{p.Version}");
            }
            stream.Flush(); // Empty the stream to the base stream
            // Don't close the StreamWriter, this will close the base stream
            baseStream.Seek(0, SeekOrigin.Begin); // Move the position to the start of the stream
            return baseStream;
        }
    }
}
