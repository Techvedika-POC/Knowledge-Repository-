using Knowledge_Repository.Application.Interfaces.Services;
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

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Retrieves all events.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        /// <summary>
        /// Retrieves a single event by ID.
        /// </summary>
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

        /// <summary>
        /// Retrieves events by event type.
        /// </summary>
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

        /// <summary>
        /// Creates a new event.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] Event evt)
        {
            if (evt == null)
                return BadRequest(new { success = false, message = "Event payload is required." });

            try
            {
                // Ensure EventId is new for creation
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

        /// <summary>
        /// Updates an existing event by ID.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] Event evt)
        {
            if (evt == null)
                return BadRequest(new { success = false, message = "Event payload is required." });

            if (id == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            // If the frontend forgot to send eventId, ensure it’s set from URL
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

        /// <summary>
        /// Deletes an event by ID.
        /// </summary>
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
    }
}
