using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class EventService : IEventService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EventService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        //Get all events Available
        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Select(e => new Event
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    OwnerId = e.OwnerId
                })
                .ToListAsync();
        }
        //Get events by type of the event
        public async Task<List<Event>> GetEventsByTypeAsync(string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
                return new List<Event>();

            return await _context.Events
                .Where(e => e.EventType.ToLower() == eventType.ToLower())
                .Select(e => new Event
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    OwnerId = e.OwnerId,
                    EventType = e.EventType
                })
                .ToListAsync();
        }



    }
}
