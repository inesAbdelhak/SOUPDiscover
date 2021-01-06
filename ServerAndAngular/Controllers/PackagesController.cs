﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoupDiscover.Common;
using SoupDiscover.Dto;
using SoupDiscover.ORM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<ActionResult<IEnumerable<Package>>> GetPackages([FromQuery] string projectName, [FromQuery] string csproj)
        {
            var packages = _context.PackageConsumerPackages.Where(p => true);
            if (!string.IsNullOrEmpty(projectName))
            {
                packages = packages.Where(p => p.PackageConsumer.Project.Name == projectName);
            }
            if (!string.IsNullOrEmpty(csproj))
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

        /// <summary>
        /// Export package into a csv file
        /// </summary>
        /// <param name="projectId">the project to export</param>
        /// <returns>A result with</returns>
        [HttpGet("exporttocsvfromproject/{projectId}")]
        public async Task<ActionResult> DownloadCsvFromProject(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return Problem($"Project Id not defined!");
            }
            var csvStream = ExportPackagesFromProject(projectId);
            if (csvStream == null)
            {
                return Problem($"Unable to find the project {projectId}");
            }
            return File(csvStream, "text/plain", $"{projectId}.csv");
        }

        /// <summary>
        /// Search all packages in all projects that id contains parameter packageId.
        /// Export packages into a csv file.
        /// </summary>
        /// <param name="packageId">A part of the packageid</param>
        /// <returns>A result with</returns>
        [HttpGet("exporttocsvfromid/{packageId}")]
        public async Task<ActionResult> DownloadCsvFromId(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                return Problem($"Package Id not defined!");
            }
            var csvStream = ExportPackagesFromId(packageId);
            if (csvStream == null)
            {
                return Problem($"Unable to find the project {packageId}");
            }
            return File(csvStream, "text/plain", $"{packageId}.csv");
        }

        [HttpGet("searchpackage/{packageId}")]
        public async Task<ActionResult<IEnumerable<PackageWithProjectDto>>> SearchPackageFromId(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                return Problem($"Package Id not defined!");
            }
            var package = SearchPackage(packageId).ToList();
            return package;
        }

        public IEnumerable<PackageWithProjectDto> SearchPackage(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                throw new ApplicationException($"You must set at least 3 char for the parameter {nameof(packageId)}.");
            }
            var packagesFound = from packageConsumer in _context.PackageConsumer
                                join p2 in _context.PackageConsumerPackages on packageConsumer.PackageConsumerId equals p2.PackageConsumerId
                                where p2.Package.PackageId.Contains(packageId)
                                select new { packageConsumer, p2.Package };
            var packageDico = new Dictionary<Package, List<PackageConsumer>>();
            foreach (var package in packagesFound)
            {
                List<PackageConsumer> list;
                if (!packageDico.ContainsKey(package.Package))
                {
                    list = new List<PackageConsumer>();
                    packageDico.Add(package.Package, list);
                }
                else
                {
                    list = packageDico[package.Package];
                }
                list.Add(package.packageConsumer);
            }
            return packageDico.Select(p => new PackageWithProjectDto(p.Key, p.Value.Select(p => p.ToDto()).ToArray()));
        }

        /// <summary>
        /// Create a CVS of packages found
        /// </summary>
        /// <param name="packageId">The package id must contains</param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        private Stream ExportPackagesFromId(string packageId, char delimiter = ';')
        {
            var packages = SearchPackage(packageId);
            var baseStream = new MemoryStream();
            var stream = new StreamWriter(baseStream, System.Text.Encoding.UTF8)
            {
                NewLine = "\r\n"// RFC 4180
            };
            // Create header
            stream.WriteLine(CSVFileHelper.SerializeToCvsLine(new string[] { "PackageId", "Version", "Type", "License", "sous-projet", "Projets" }, delimiter));
            foreach (var p in packages)
            {
                foreach (var c in p.packageConsumers)
                {
                    stream.WriteLine(CSVFileHelper.SerializeToCvsLine(new string[]
                    {
                        p.packageDto.PackageId,
                        p.packageDto.Version,
                        p.packageDto.PackageType.ToString(),
                        p.packageDto.Licence,
                        c.name,
                        c.projectId,
                    },
                    delimiter));
                }
            }
            stream.Flush(); // Empty the stream to the base stream
            // Don't close the StreamWriter, this will close the base stream
            baseStream.Seek(0, SeekOrigin.Begin); // Move the position to the start of the stream
            return baseStream;
        }

        /// <summary>
        /// Create a cvs file that contains all packages of project
        /// Create a stream that contains the csv file
        /// </summary>
        /// <param name="projectId">Id of the project</param>
        private Stream ExportPackagesFromProject(string projectId, char delimiter = ';')
        {
            var project = _context.Projects.Find(projectId);
            if (project == null)
            {
                return null;
            }
            var baseStream = new MemoryStream();
            var stream = new StreamWriter(baseStream, System.Text.Encoding.UTF8)
            {
                NewLine = "\r\n"// RFC 4180
            };
            _context.Entry(project).Collection(p => p.PackageConsumers).Load();

            // Create header
            stream.WriteLine(CSVFileHelper.SerializeToCvsLine(new string[] { "PackageId", "Version", "Type", "Description", "License" }, delimiter));
            var packages = _context.PackageConsumerPackages.Where(p => p.PackageConsumer.ProjectId == projectId).Select(p => p.Package).Distinct();
            foreach (var p in packages)
            {
                stream.WriteLine(CSVFileHelper.SerializeToCvsLine(new string[] { p.PackageId, p.Version, p.PackageType.ToString(), p.Description, p.Licence }, delimiter));
            }
            stream.Flush(); // Empty the stream to the base stream
            // Don't close the StreamWriter, this will close the base stream
            baseStream.Seek(0, SeekOrigin.Begin); // Move the position to the start of the stream
            return baseStream;
        }
    }
}
