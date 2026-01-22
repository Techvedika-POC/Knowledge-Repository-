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
        public async Task<List<Event>> GetCurrentIdeathonsAsync(DateTime todayUtc)
        {
            var today = DateOnly.FromDateTime(todayUtc);

            return await _context.Events
                .Where(e =>
                    e.EventType.Trim().ToLower() == "ideathon"
                    && e.StartDate.HasValue
                    && e.EndDate.HasValue
                    && e.StartDate.Value <= today
                    && e.EndDate.Value >= today
                )
                .AsNoTracking()
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }


        public async Task<List<Event>> GetIdeathonsForMonthAsync(int year, int month)
        {
            if (month < 1 || month > 12) return new List<Event>();

            var monthStart = new DateOnly(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1); 

            return await _context.Events
                .Where(e => e.EventType == "Ideathon"
                            && e.StartDate.HasValue
                            && e.EndDate.HasValue
                            && e.StartDate.Value <= monthEnd
                            && e.EndDate.Value >= monthStart)
                .AsNoTracking()
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }
    }
}
