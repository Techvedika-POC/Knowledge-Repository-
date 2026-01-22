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
    public class JuryChatRepository : GenericRepository<JuryChatMessage>, IJuryChatRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public JuryChatRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JuryChatMessage>> GetByEventAsync(Guid eventId, int limit = 100)
        {
            return await _context.JuryChatMessages
                .Where(m => m.EventId == eventId)
                .Include(m => m.SenderJury)
                .Include(m => m.ReplyToMessage)
                    .ThenInclude(r => r.SenderJury)

                .OrderBy(m => m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }



    }
}
