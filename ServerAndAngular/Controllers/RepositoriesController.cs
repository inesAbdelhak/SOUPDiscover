using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoupDiscover.Core.Respository;
using SoupDiscover.ORM;

namespace SoupDiscover.Controllers
{
    public enum RepositoryType
    {
        None,
        Git,
    }
    public class RepositoryDto
    {
        public RepositoryType repositoryType { get; set; }

        /// <summary>
        /// The name of the repository (to display)
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// The name of sh key to clone the repository
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// The name of sh key to clone the repository
        /// </summary>
        public string sshKeyName { get; set; }

        /// <summary>
        /// The name of the branch to process
        /// </summary>
        public string branch { get; set; }

    }

    [Route("api/[controller]")]
    [ApiController]
    public class RepositoriesController : ControllerBase
    {
        private readonly DataContext _context;

        public RepositoriesController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Repositories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepositoryDto>>> GetRepository()
        {
            var list = await _context.Repository.ToListAsync();
            var dtos = list.Select(r =>
            {
                var git = (GitRepository)r;
                _context.Entry(git).Reference(g => g.SshKey).LoadAsync();
                return new RepositoryDto()
                {
                    repositoryType = RepositoryType.Git,
                    branch = git.Branch,
                    name = r.Name,
                    sshKeyName = git.SshKeyId,
                    url = git.Url,
                };
            });
            return CreatedAtAction("GetAllRepositories", dtos);
        }

        // GET: api/Repositories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Repository>> GetRepository(string id)
        {
            var repository = await _context.Repository.FindAsync(id);

            if (repository == null)
            {
                return NotFound();
            }

            return repository;
        }

        // PUT: api/Repositories/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRepository(string id, Repository repository)
        {
            if (id != repository.Name)
            {
                return BadRequest();
            }

            _context.Entry(repository).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RepositoryExists(id))
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

        // POST: api/Repositories
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<RepositoryDto>> PostRepository(RepositoryDto repositoryDto)
        {
            Repository repository = null;
            // Create repository from repository dto
            switch (repositoryDto.repositoryType)
            {
                case RepositoryType.Git:
                    var sshKey = _context.Credentials.Find(repositoryDto.sshKeyName);
                    repository = new GitRepository()
                    {
                        Branch = repositoryDto.branch,
                        Url = repositoryDto.url,
                        Name = repositoryDto.name,
                        SshKey = sshKey,
                        SshKeyId = sshKey?.name,
                    };
                break;
            }
            if (repository == null)
            {
                return UnprocessableEntity($"The repository type {repositoryDto.repositoryType} is not supported!");
            }
            _context.Repository.Add(repository);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRepository", new { id = repository.Name }, repository);
        }

        // DELETE: api/Repositories/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Repository>> DeleteRepository(int id)
        {
            var repository = await _context.Repository.FindAsync(id);
            if (repository == null)
            {
                return NotFound();
            }

            _context.Repository.Remove(repository);
            await _context.SaveChangesAsync();

            return repository;
        }

        private bool RepositoryExists(string id)
        {
            return _context.Repository.Any(e => e.Name == id);
        }
    }
}
