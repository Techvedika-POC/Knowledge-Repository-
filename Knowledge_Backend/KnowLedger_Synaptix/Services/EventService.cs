using KnowLedger_Synaptix.Dtos;
using Microsoft.EntityFrameworkCore;
using KnowLedger_Synaptix.Models;

namespace KnowLedger_Synaptix.Services
{
    public class EventService : IEventService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EventService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<List<EventDto>> GetAllEventsAsync()
        {
            return await _context.Events
                .Select(e => new EventDto
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
    }
}
