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
    }
}
