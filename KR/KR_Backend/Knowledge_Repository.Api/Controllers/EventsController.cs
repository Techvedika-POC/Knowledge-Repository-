using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Application.Dtos.EventInsight;
using Knowledge_Repository.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            if (string.IsNullOrWhiteSpace(eventType))
                return BadRequest(new { success = false, message = "Event type is required." });

            var events = await _eventService.GetEventsByTypeAsync(eventType);
            return (events == null || events.Count == 0)
                ? NotFound(new { success = false, message = $"No events found for type: {eventType}" })
                : Ok(events);
        }

        // ======================================================
        //               NEW ADDED ENDPOINTS
        // ======================================================

        /// <summary>
        /// Returns events that belong to the current month.
        /// </summary>
        [HttpGet("current-month")]
        public async Task<IActionResult> GetCurrentMonthEvents()
        {
            try
            {
                var events = await _eventService.GetCurrentMonthEventsAsync();
                return Ok(new { success = true, data = events });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to fetch current month events.", details = ex.Message });
            }
        }

        /// <summary>
        /// Returns events that are active today.
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveEvents()
        {
            try
            {
                var events = await _eventService.GetActiveEventsAsync();
                return Ok(new { success = true, data = events });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to fetch active events.", details = ex.Message });
            }
        }

        // ======================================================
        //               EXISTING CRUD ENDPOINTS
        // ======================================================

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

        // ======================================================
        //               INSIGHT ENDPOINTS
        // ======================================================

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
                return BadRequest(new { success = false, message = "Invalid event or user ID." });

            try
            {
                var insight = await _insightService.GetUserEventInsightAsync(userId, eventId);
                return Ok(new { success = true, data = insight });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to fetch user event insight.", details = ex.Message });
            }
        }
    }
}
