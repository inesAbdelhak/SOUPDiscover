using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoupDiscover.Dto;
using SoupDiscover.ORM;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoupDiscover.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CredentialsController : ControllerBase
    {
        private readonly DataContext _context;

        public CredentialsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Credentials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CredentialDto>>> GetCredentials()
        {
            var list = await _context.Credentials.ToArrayAsync();
            return list.Select(e => e.ToDto(true)).ToArray(); // doesn't return the key value
        }

        // GET: api/Credentials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CredentialDto>> GetCredential(string id)
        {
            var credential = await _context.Credentials.FindAsync(id);

            if (credential == null)
            {
                return NotFound();
            }

            return credential.ToDto(true);
        }

        // PUT: api/Credentials/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCredential(string id, Credential credential)
        {
            if (id != credential.Name)
            {
                return BadRequest();
            }

            var error = CheckCredential(credential);
            if (error != null)
            {
                return Problem(error);
            }

            _context.Entry(credential).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CredentialExists(id))
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
        /// Check the validity of credential to add or update.
        /// Return a string that contains the error to send to user.
        /// </summary>
        /// <param name="credential">The credential to check</param>
        /// <returns>null : No errors, a string : The error message to send</returns>
        private string CheckCredential(Credential credential)
        {
            switch (credential.CredentialType)
            {
                case CredentialType.SSH:
                    if (string .IsNullOrEmpty(credential.Key))
                    {
                        return $"A credential of type {credential.CredentialType} must contains a key";
                    }
                    break;
                case CredentialType.Token:
                    if (string.IsNullOrEmpty(credential.Token))
                    {
                        return $"A credential of type {credential.CredentialType} must contains a token";
                    }
                    break;
                case CredentialType.Password:
                    if (string.IsNullOrEmpty(credential.Login))
                    {
                        return $"A credential of type {credential.CredentialType} must contains a login";
                    }
                    break;
            }
            return null;
        }

        // POST: api/Credentials
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Credential>> PostCredential(Credential credential)
        {
            var error = CheckCredential(credential);
            if (error != null)
            {
                return Problem(error);
            }
            _context.Credentials.Add(credential);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CredentialExists(credential.Name))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCredential", new { id = credential.Name }, credential);
        }

        // DELETE: api/Credentials/5
        [HttpDelete("{credentialId}")]
        public async Task<ActionResult<CredentialDto>> DeleteCredential(string credentialId)
        {
            var credentialTask = _context.Credentials.FindAsync(credentialId);
            var repoOfTheCredentialTask = _context.Repositories.OfType<GitRepository>().Where(e => e.CredentialId == credentialId).Select(e => e.Name).ToArrayAsync();
            var credential = await credentialTask;
            if (credential == null)
            {
                return NotFound();
            }

            var repoOfTheCredential = await repoOfTheCredentialTask;

            if (repoOfTheCredential.Length > 0)
            {
                return Problem($"Supprimer les dépots {string.Join(",", repoOfTheCredential)} avant de pouvoir supprimer la clée ssh {credentialId}.");
            }

            _context.Credentials.Remove(credential);
            await _context.SaveChangesAsync();

            return credential.ToDto();
        }

        private bool CredentialExists(string id)
        {
            return _context.Credentials.Any(e => e.Name == id);
        }
    }
}
