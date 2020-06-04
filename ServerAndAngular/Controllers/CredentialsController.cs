using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoupDiscover.Dto;
using SoupDiscover.ORM;

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
            var list = await _context.Credentials.Select(e => e.Name).ToArrayAsync();
            return list.Select(e => new CredentialDto() { Name = e, Key = "*****" }).ToArray(); // doesn't return the key value
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

            return credential.ToDto();
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

        // POST: api/Credentials
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Credential>> PostCredential(Credential credential)
        {
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
            var repoOfTheCredentialTask = _context.Repositories.OfType<GitRepository>().Where(e => e.SshKeyId == credentialId).Select(e => e.Name).ToArrayAsync();
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
