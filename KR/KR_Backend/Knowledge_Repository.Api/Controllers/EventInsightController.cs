using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Application.Dtos.EventInsight;
using Knowledge_Repository.Application.Dtos.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        [HttpGet("{eventId:guid}/user/{userId:guid}/insight")]
        public async Task<IActionResult> GetUserEventInsight(Guid eventId, Guid userId)
        {
            if (eventId == Guid.Empty || userId == Guid.Empty)
                return BadRequest(ApiResponse<object>.Fail(
                    "Invalid event or user. Please refresh and try again."
                ));

            try
            {
                var insight = await _insightService.GetUserEventInsightAsync(userId, eventId);

                return Ok(ApiResponse<UserEventInsightDto>.Ok(
                    insight,
                    "Insight loaded successfully."
                ));
            }
            catch (KeyNotFoundException)
            {
                return Ok(ApiResponse<object>.Ok(
                    null,
                    "You have not joined a team for this event yet."
                ));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Fail(
                    "We couldn’t load your event details right now. Please try again later."
                ));
            }
        }

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetEventInsight(Guid eventId)
        {
            if (eventId == Guid.Empty)
                return BadRequest(ApiResponse<object>.Fail("Invalid event ID."));

            try
            {
                var insight = await _insightService.GetEventInsightsAsync(eventId);
                return Ok(ApiResponse<object>.Ok(insight));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Fail(
                    "Failed to load event insights."
                ));
            }
        }
    }
}
