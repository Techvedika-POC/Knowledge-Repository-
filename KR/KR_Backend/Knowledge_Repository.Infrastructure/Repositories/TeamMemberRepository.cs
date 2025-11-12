using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class TeamMemberRepository : GenericRepository<TeamMember>, ITeamMemberRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public TeamMemberRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsUserRegisteredForEventAsync(Guid userId, Guid eventId)
        {
            return await _context.TeamMembers
                .Include(tm => tm.Team) 
                .AnyAsync(tm => tm.UserId == userId && tm.Team.EventId == eventId);
        }
    }
}
