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


        // -----------------------------------------------------------
        // NEW METHODS for Dashboards
        // -----------------------------------------------------------

        /// <summary>
        /// Gets all events for the current month (Start OR End date falls within this month)
        /// </summary>
        public async Task<List<Event>> GetCurrentMonthEventsAsync()
        {
            var today = DateTime.Today;
            var firstDay = new DateOnly(today.Year, today.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            return await _context.Events
                .Where(e =>
                    (e.StartDate.HasValue && e.StartDate.Value >= firstDay && e.StartDate.Value <= lastDay) ||
                    (e.EndDate.HasValue && e.EndDate.Value >= firstDay && e.EndDate.Value <= lastDay)
                )
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Gets active events for the current date (Running events)
        /// </summary>
        public async Task<List<Event>> GetActiveEventsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            return await _context.Events
                .Where(e =>
                    e.StartDate.HasValue &&
                    e.EndDate.HasValue &&
                    e.StartDate.Value <= today &&
                    e.EndDate.Value >= today
                )
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
