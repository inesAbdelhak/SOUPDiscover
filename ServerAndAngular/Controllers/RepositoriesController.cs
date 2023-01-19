using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoupDiscover.Dto;
using SoupDiscover.ORM;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SoupDiscover.Core.Repository;

namespace SoupDiscover.Controllers
{
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
            var list = await _context.Repositories.ToListAsync();
            var dtos = list.Select(r => r.ToDto());
            return CreatedAtAction("GetAllRepositories", dtos);
        }

        // GET: api/Repositories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RepositoryDto>> GetRepository(string id)
        {
            var repository = await _context.Repositories.FindAsync(id);

            if (repository == null)
            {
                return NotFound();
            }

            return repository.ToDto();
        }

        // PUT: api/Repositories/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <summary>
        /// Update a repository
        /// </summary>
        /// <param name="id">The id of the repository to update</param>
        /// <param name="repositorydto">all fields to update</param>        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRepository(string id, RepositoryDto repositorydto)
        {

            if (id != repositorydto.name)
            {
                return BadRequest();
            }

            var errors = CheckRepository(repositorydto, _context);
            if (errors != null)
            {
                return Problem(errors);
            }

            var repository = repositorydto.ToModel();
            _context.Repositories.Update(repository);

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

        /// <summary>
        /// Check all fields in RepositoryDto object.
        /// </summary>
        /// <param name="repository">The repository to check</param>
        /// <param name="context">The database context, to check elements in database</param>
        /// <returns>a string withe errors founds, or null.</returns>
        private string CheckRepository(RepositoryDto repository, DataContext context)
        {
            var errors = string.Empty;            
            switch (repository.repositoryType)
            {
                
                case RepositoryType.Git:
                    var r = new Regex(@"^(https?:\/\/(www\.)?|git@)[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}([-a-zA-Z0-9()!@:%_\+.~#?&\/\/=]*)$");
                    if (string.IsNullOrEmpty(repository.name))
                    {
                        errors += $"The {nameof(repository.name)} must be not empty. ";
                    }
                    if (string.IsNullOrEmpty(repository.branch))
                    {
                        errors += $"The {nameof(repository.branch)} must be not empty. ";
                    }
                    if (string.IsNullOrEmpty(repository.credentialId))
                    {
                        errors += $"The {nameof(repository.credential)} must be set. ";
                    }
                    if(!r.IsMatch(repository.url))
                    {
                        errors += $"The {nameof(repository.url)} '{nameof(repository.url)}' is bad formed.";
                    }
                    else
                    {
                        // here the url is well formed
                        var credential = context.Credentials.Find(repository.credentialId);
                        if (credential == null)
                        {
                            errors += $"The {nameof(repository.credentialId)} is not found in database. ";
                        }
                        if (credential != null && credential.CredentialType == CredentialType.SSH && repository.GetUrlScheme().StartsWith("http"))
                        {
                            errors += $"The credential type {credential.CredentialType} not compatible with url schema {repository.GetUrlScheme()}. ";
                        }

                        if (credential != null && credential.CredentialType != CredentialType.SSH && !repository.GetUrlScheme().StartsWith("http"))
                        {
                            errors += $"The credential type {credential.CredentialType} not compatible with url schema {repository.GetUrlScheme()}. ";
                        }
                    }
                    break;
                    
                default:
                    errors = $"The repository type {repository.repositoryType} not supported. ";
                    break;
            }
            if (errors == string.Empty)
            {
                return null;
            }
            return errors;
        }

        // POST: api/Repositories
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<RepositoryDto>> PostRepository(RepositoryDto repositoryDto)
        {
            if (repositoryDto == null)
            {
                return UnprocessableEntity($"The repository type {repositoryDto.repositoryType} is not supported!");
            }
            var repository = repositoryDto.ToModel();
            if (repository == null)
            {
                return UnprocessableEntity($"The repository type {repositoryDto.repositoryType} is not supported!");
            }
            var errors = CheckRepository(repositoryDto, _context);
            if(errors != null)
            {
                return Problem(errors);
            }
            _context.Repositories.Add(repository);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRepository", new { id = repository.Name }, repository);
        }

        // DELETE: api/Repositories/5
        /// <summary>
        /// Delete a repository
        /// </summary>
        /// <param name="repositoryId">Id of the repository to delete</param>
        [HttpDelete("{repositoryId}")]
        public async Task<ActionResult<RepositoryDto>> DeleteRepository(string repositoryId)
        {
            var repositoryTask = _context.Repositories.FindAsync(repositoryId);
            var projectOfTheRepositoryTask = _context.Projects.Where(p => p.RepositoryId == repositoryId).ToArrayAsync();
            var repository = await repositoryTask;
            if (repository == null)
            {
                return NotFound();
            }
            var projectOfTheRepository = await projectOfTheRepositoryTask;
            if (projectOfTheRepository.Length > 0)
            {
                return Problem($"Il faut suprimer les projets {string.Join(",", projectOfTheRepository.Select(e => e.RepositoryId))} avant de supprimer le dépot {repositoryId}.");
            }

            _context.Repositories.Remove(repository);
            await _context.SaveChangesAsync();
            var repositoryDto = repository.ToDto();
            if (repository == null)
            {
                return Problem($"Repository type {typeof(Repository)} is not supported!");
            }
            return repositoryDto;
        }

        private bool RepositoryExists(string id)
        {
            return _context.Repositories.Any(e => e.Name == id);
        }
    }
}
