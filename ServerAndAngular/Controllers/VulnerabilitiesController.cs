using SoupDiscover.Dto;
using SoupDiscover.ORM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    
using GraphQL;
using System.Threading.Tasks;
using GraphQL.Types;
using SoupDiscover.GraphQL;
using System.Collections.Generic;


namespace SoupDiscover.Controllers
{

   /* [Route("")]
    [ApiController]
    public class VulnerabilitiesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ISchema _schema;
        private readonly IDocumentExecuter _executer;

        public VulnerabilitiesController(ISchema schema, IDocumentExecuter executer)
        {
            _schema = schema;
            _executer = executer;
        }

       

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GraphQLQuery query)
        {
            var result = await new DocumentExecuter().ExecuteAsync(options =>
            {
                options.Schema = _schema;
                options.Query = query.Query; 
            });

            if (result.Errors?.Count > 0)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }





    }*/





}
