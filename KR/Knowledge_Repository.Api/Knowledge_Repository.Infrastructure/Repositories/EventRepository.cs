using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EventRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events.AsNoTracking().ToListAsync();
        }

        public async Task<List<Event>> GetEventsByTypeAsync(string eventType)
        {
            return await _context.Events
                .Where(e => e.EventType.ToLower() == eventType.ToLower())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(Guid eventId)
        {
            return await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);
        }

        public async Task UpdateAsync(Event evt)
        {
            _context.Events.Update(evt);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Event evt)
        {
            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();
        }
    }
}
