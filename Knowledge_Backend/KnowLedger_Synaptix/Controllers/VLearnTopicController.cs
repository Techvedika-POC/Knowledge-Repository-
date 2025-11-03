using Microsoft.AspNetCore.Mvc;
using KnowLedger_Synaptix.Services.Interfaces;

namespace KnowLedger_Synaptix.Controllers
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
