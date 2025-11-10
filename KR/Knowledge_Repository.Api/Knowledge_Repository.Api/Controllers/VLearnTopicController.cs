using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VLearnTopicController : ControllerBase
    {
        private readonly IVLearnTopicService _vlearnTopicService;

        public VLearnTopicController(IVLearnTopicService vlearnTopicService)
        {
            _vlearnTopicService = vlearnTopicService;
        }

        /// <summary>
        /// Retrieves all learning topics.
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllTopics()
        {
            var topics = await _vlearnTopicService.GetAllTopicsAsync();

            if (topics == null || !topics.Any())
                return NotFound("No topics found.");

            return Ok(topics);
        }
    }
}
