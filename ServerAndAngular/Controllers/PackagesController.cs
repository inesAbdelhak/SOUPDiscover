using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoupDiscover.Common;
using SoupDiscover.ORM;

namespace SoupDiscover.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly DataContext _context;

        public PackagesController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Packages/Filter?projectName=Test
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Package>>> GetPackages([FromQuery]string projectName, [FromQuery]string csproj)
        {
            var packages = _context.PackageConsumerPackages.Where(p => true);
            if(!string.IsNullOrEmpty(projectName))
            {
                packages = packages.Where(p => p.PackageConsumer.Project.Name == projectName);
            }
            if(!string.IsNullOrEmpty(csproj))
            {
                packages = packages.Where(p => p.PackageConsumer.Name == csproj);
            }
            return await packages.Select(p => p.Package).Distinct().ToListAsync();
        }

        // GET: api/Packages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Package>> GetPackage(int id)
        {
            var package = await _context.Packages.FindAsync(id);

            if (package == null)
            {
                return NotFound();
            }

            return package;
        }

        // PUT: api/Packages/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPackage(int id, Package package)
        {
            if (id != package.Id)
            {
                return BadRequest();
            }

            _context.Entry(package).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PackageExists(id))
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

        // POST: api/Packages
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Package>> PostPackage(Package package)
        {
            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPackage", new { id = package.Id }, package);
        }

        // DELETE: api/Packages/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Package>> DeletePackage(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            _context.Packages.Remove(package);
            await _context.SaveChangesAsync();

            return package;
        }

        private bool PackageExists(int id)
        {
            return _context.Packages.Any(e => e.Id == id);
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
            _context.Entry(project).Collection(p => p.PackageConsumers).Load();

            // Create header
            stream.WriteLine(CVSFileHlper.ConvertToCvsLine(new string[] { "PackageId", "Version", "Type", "Description", "License" }, delimiter));
            var packages = _context.PackageConsumerPackages.Where(p => p.PackageConsumer.ProjectId == projectId).Select(p => p.Package).Distinct();
            foreach (var p in packages)
            {
                stream.WriteLine(CVSFileHlper.ConvertToCvsLine(new string[] { p.PackageId, p.Version, p.PackageType.ToString(), p.Description, p.Licence }, delimiter));
            }
            stream.Flush(); // Empty the stream to the base stream
            // Don't close the StreamWriter, this will close the base stream
            baseStream.Seek(0, SeekOrigin.Begin); // Move the position to the start of the stream
            return baseStream;
        }
    }
}
