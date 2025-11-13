using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopicHighlightController : ControllerBase
    {
        private readonly ITopicHighlightService _topicService;

        public TopicHighlightController(ITopicHighlightService topicService)
        {
            _topicService = topicService;
        }

        [HttpGet("topics")]
        public async Task<IActionResult> GetTopics([FromQuery] int top = 10)
        {
            var result = await _topicService.GetTopicHighlightsAsync(top);
            return Ok(result);
        }

        [HttpGet("knowledge")]
        public async Task<IActionResult> GetKnowledgeItems([FromQuery] string domain, [FromQuery] int top = 10)
        {
            if (string.IsNullOrEmpty(domain))
                return BadRequest("Domain parameter is required.");

            var items = await _topicService.GetKnowledgeItemsByDomainAsync(domain, top);
            return Ok(items);
        }
    }
}
