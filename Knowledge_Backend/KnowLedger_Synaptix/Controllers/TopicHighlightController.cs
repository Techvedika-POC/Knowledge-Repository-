using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
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

        /// <summary>
        /// Get top topic highlights (domains) for display
        /// </summary>
        [HttpGet("topics")]
        public async Task<IActionResult> GetTopics([FromQuery] int top = 10)
        {
            var result = await _topicService.GetTopicHighlightsAsync(top);
            return Ok(result);
        }

        /// <summary>
        /// Get top knowledge items for a selected domain/topic
        /// </summary>
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
