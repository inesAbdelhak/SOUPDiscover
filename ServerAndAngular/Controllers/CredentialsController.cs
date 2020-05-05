using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<IEnumerable<Credential>>> GetCredentials()
        {
            return await _context.Credentials.ToListAsync();
        }

        // GET: api/Credentials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Credential>> GetCredential(string id)
        {
            var authentificationToken = await _context.Credentials.FindAsync(id);

            if (authentificationToken == null)
            {
                return NotFound();
            }

            return authentificationToken;
        }

        // PUT: api/Credentials/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCredential(string id, Credential credential)
        {
            if (id != credential.name)
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
        public async Task<ActionResult<Credential>> PostCredential(Credential authentificationToken)
        {
            _context.Credentials.Add(authentificationToken);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CredentialExists(authentificationToken.name))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetAuthentificationToken", new { id = authentificationToken.name }, authentificationToken);
        }

        // DELETE: api/Credentials/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Credential>> DeleteCredential(string id)
        {
            var authentificationToken = await _context.Credentials.FindAsync(id);
            if (authentificationToken == null)
            {
                return NotFound();
            }

            _context.Credentials.Remove(authentificationToken);
            await _context.SaveChangesAsync();

            return authentificationToken;
        }

        private bool CredentialExists(string id)
        {
            return _context.Credentials.Any(e => e.name == id);
        }
    }
}
