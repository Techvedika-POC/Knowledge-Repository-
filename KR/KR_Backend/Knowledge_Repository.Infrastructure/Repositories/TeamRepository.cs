using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public TeamRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Team?> GetTeamByEventAndUserAsync(Guid eventId, Guid userId)
        {
            return await _context.Teams
                .Include(t => t.TeamMembers) 
                .FirstOrDefaultAsync(t => t.EventId == eventId &&
                                          t.TeamMembers.Any(tm => tm.UserId == userId));
        }
        public async Task<IEnumerable<User>> GetTeamMembersAsync(Guid teamId)
        {
            return await _context.TeamMembers
                .Include(tm => tm.User)
                .Where(tm => tm.TeamId == teamId)
                .Select(tm => tm.User)
                .ToListAsync();
        }
        public async Task<List<Team>> GetTeamsByEventAsync(Guid eventId)
        {
            return await _context.Teams
                .Where(t => t.EventId == eventId)
                .ToListAsync();
        }
        public async Task<List<Team>> GetByEventIdAsync(Guid eventId)
        {
      
            return await _context.Teams
                .Where(t => t.EventId == eventId)
                .ToListAsync();
        }
    }
}
