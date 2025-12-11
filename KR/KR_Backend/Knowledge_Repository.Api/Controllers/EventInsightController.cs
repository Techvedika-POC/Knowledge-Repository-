using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Knowledge_Repository.Application.Dtos.EventInsight;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventInsightController : ControllerBase
    {
        private readonly IEventTeamInsightService _insightService;

        public EventInsightController(IEventTeamInsightService insightService)
        {
            _insightService = insightService;
        }

        [HttpGet("user/{userId}/event/{eventId}")]
        public async Task<IActionResult> GetUserEventInsight(Guid userId, Guid eventId)
        {
            if (userId == Guid.Empty || eventId == Guid.Empty)
                return BadRequest("Invalid user or event ID.");

            try
            {
                var insight = await _insightService.GetUserEventInsightAsync(userId, eventId);
                return Ok(insight);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetEventInsight(Guid eventId)
        {
            if (eventId == Guid.Empty)
                return BadRequest("Invalid event ID.");

            try
            {
                var insight = await _insightService.GetEventInsightsAsync(eventId);
                if (insight == null)
                    return NotFound("No insights found for this event.");

                return Ok(insight);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load event insights.", detail = ex.Message });
            }
        }
   
    }
}