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
   public class EventJuryRepository:GenericRepository<EventJury>,IEventJuryRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EventJuryRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> IsUserEventJuryAsync(Guid userId)
        {
            return await _context.EventJuries
                .AnyAsync(ej => ej.UserId == userId);
        }
        public async Task<bool> IsUserJuryForEventAsync(Guid eventId, Guid userId)
        {
            return await _context.EventJuries
                .AnyAsync(ej => ej.UserId == userId && ej.EventId == eventId);
        }

        public async Task<List<Guid>> GetEventIdsForUserAsync(Guid userId)
        {
            return await _context.EventJuries
                .Where(ej => ej.UserId == userId)
                .Select(ej => ej.EventId)
                .ToListAsync();
        }
    }
}
