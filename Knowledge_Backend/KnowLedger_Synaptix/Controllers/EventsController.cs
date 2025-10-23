using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KnowLedger_Synaptix.Controllers
{
    // Handles event-related endpoints, like retrieving all events.
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }
        //Getting all events
        [HttpGet]
        public async Task<ActionResult<List<Event>>> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }
        //Getting the events by type 
        [HttpGet("type/{eventType}")]
        public async Task<ActionResult<List<Event>>> GetEventsByType(string eventType)
        {
            var events = await _eventService.GetEventsByTypeAsync(eventType);
            if (events == null || events.Count == 0)
                return NotFound($"No events found for type: {eventType}");
            return Ok(events);
        }

    }
}
