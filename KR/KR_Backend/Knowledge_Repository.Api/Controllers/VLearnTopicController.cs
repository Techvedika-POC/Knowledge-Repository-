using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
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

        // GET api/vlearntopic/all
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTopics()
        {
            var topics = await _vlearnTopicService.GetAllTopicsAsync();

            if (topics == null || !topics.Any())
                return NotFound("No topics found.");

            return Ok(topics);
        }

        // GET api/vlearntopic/{topicId}
        [HttpGet("{topicId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopicById(Guid topicId)
        {
            var topic = await _vlearnTopicService.GetTopicByIdAsync(topicId);
            if (topic == null) return NotFound();
            return Ok(topic);
        }

        // POST api/vlearntopic (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTopic([FromBody] CreateTopicDto dto)
        {
            if (dto == null) return BadRequest("Request body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var createdBy = GetCurrentUserId();
                var created = await _vlearnTopicService.AddTopicAsync(dto, createdBy);
                return CreatedAtAction(nameof(GetTopicById), new { topicId = created.TopicId }, created);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (InvalidOperationException iex)
            {
                return Conflict(iex.Message);
            }
            catch (Exception ex)
            {
                // Optional: log exception
                return StatusCode(500, ex.Message);
            }
        }

        private Guid GetCurrentUserId()
        {
            var idClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "user_id");
            if (idClaim == null) return Guid.Empty;
            return Guid.TryParse(idClaim.Value, out var g) ? g : Guid.Empty;
        }
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string q = "", [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            if (page < 1) page = 1;
            if (size < 1 || size > 100) size = 10;

            var (items, total) = await _vlearnTopicService.SearchTopicsAsync(q, page, size);
            return Ok(new { items, total, page, size });
        }

    }
}
