using Microsoft.AspNetCore.Mvc;
using System.Net;
using WordFinder.Api.Models;

namespace WordFinder.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WordFinderController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public WordFinderController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        
        [HttpPost()]
        [Route("find")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Find([FromBody] WordFinderModel wordsRequest)
        {
            IEnumerable<string> retval = new List<string>();
            if (wordsRequest == null || wordsRequest.Matrix.Count == 0 || wordsRequest.WordStream.Count == 0)
                return BadRequest();
            try
            {
                var finder = new Logic.WordFinder(_configuration, wordsRequest.Matrix);
                retval = finder.Find(wordsRequest.WordStream);
                return Ok(retval);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
