using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Application.Dtos.EventInsight;
using Knowledge_Repository.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Knowledge_Repository.Application.Dtos.Common;


namespace Knowledge_Repository.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IEventTeamInsightService _insightService;

        public EventsController(IEventService eventService, IEventTeamInsightService insightService)
        {
            _eventService = eventService;
            _insightService = insightService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Event>> GetEventById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            var evt = await _eventService.GetEventByIdAsync(id);
            return evt is null
                ? NotFound(new { success = false, message = $"No event found with ID: {id}" })
                : Ok(evt);
        }

        [HttpGet("type/{eventType}")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEventsByType(string eventType)
        {
            var events = await _eventService.GetEventsByTypeAsync(eventType);
            return Ok(events);
        }



        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] Event evt)
        {
            if (evt == null)
                return BadRequest(new { success = false, message = "Event payload is required." });

            try
            {
                evt.EventId = Guid.NewGuid();
                evt.CreatedOn = DateTime.UtcNow;
                evt.UpdatedOn = DateTime.UtcNow;

                var created = await _eventService.AddOrUpdateEventAsync(evt);
                return CreatedAtAction(nameof(GetEventById), new { id = created.EventId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while creating the event.", details = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] Event evt)
        {
            if (evt == null)
                return BadRequest(new { success = false, message = "Event payload is required." });

            if (id == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            evt.EventId = id;

            try
            {
                evt.UpdatedOn = DateTime.UtcNow;
                var updated = await _eventService.AddOrUpdateEventAsync(evt);
                return Ok(new { success = true, message = $"Event '{updated.Title}' updated successfully.", data = updated });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating the event.", details = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            try
            {
                var success = await _eventService.DeleteEventAsync(id);
                if (!success)
                    return NotFound(new { success = false, message = $"No event found with ID: {id}" });

                return Ok(new { success = true, message = "Event deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the event.", details = ex.Message });
            }
        }


        [HttpGet("{eventId:guid}/insights")]
        public async Task<IActionResult> GetEventInsights(Guid eventId)
        {
            if (eventId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            try
            {
                var insights = await _insightService.GetEventInsightsAsync(eventId);
                return Ok(new { success = true, data = insights });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to fetch event insights.", details = ex.Message });
            }
        }

        [HttpGet("{eventId:guid}/user/{userId:guid}/insight")]
        public async Task<IActionResult> GetUserEventInsight(Guid eventId, Guid userId)
        {
            if (eventId == Guid.Empty || userId == Guid.Empty)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid event or user."
                });

            try
            {
                var insight = await _insightService.GetUserEventInsightAsync(userId, eventId);

                return Ok(new ApiResponse<UserEventInsightDto>
                {
                    Success = true,
                    Message = "Insight loaded successfully.",
                    Data = insight
                });
            }
            catch (KeyNotFoundException)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "You have not joined a team for this event yet.",
                    Data = null
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "We couldn’t load your event details right now. Please try again later."
                });
            }
        }

        [HttpGet("type/Ideathon/current")]
        public async Task<IActionResult> GetCurrentIdeathons()
        {
            try
            {
                var events = await _eventService.GetCurrentIdeathonsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load current ideathon events.", detail = ex.Message });
            }
        }
        [HttpGet("type/Ideathon/month/{year:int}/{month:int}")]
        public async Task<IActionResult> GetIdeathonsForMonth(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12.");
                var events = await _eventService.GetIdeathonsForMonthAsync(year, month);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load ideathon events for month.", detail = ex.Message });
            }
        }
        [HttpGet("hackathons/teams/count")]
        public async Task<IActionResult> GetTotalHackathonTeams()
        {
            var count = await _eventService.GetTotalHackathonTeamsAsync();
            return Ok(new { totalTeams = count });
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveEvents()
        {
            try
            {
                var events = await _eventService.GetActiveEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to load active events",
                    detail = ex.Message
                });
            }
        }

        [HttpGet("grouped-by-type-month")]
        public async Task<IActionResult> GetGroupedByTypeAndMonth()
        {
            var result = await _eventService.GetEventsGroupedByTypeAndMonthAsync();
            return Ok(result);
        }
    }
}
