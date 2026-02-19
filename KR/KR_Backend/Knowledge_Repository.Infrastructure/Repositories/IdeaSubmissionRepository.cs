using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class IdeaSubmissionRepository : IIdeaSubmissionRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public IdeaSubmissionRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<IdeaSubmission?> GetByTeamAsync(Guid teamId)
        {
            return await _context.IdeaSubmissions
                .FirstOrDefaultAsync(x => x.TeamId == teamId);
        }

        public async Task<List<IdeaSubmission>> GetByEventAsync(Guid eventId)
        {
            return await _context.IdeaSubmissions
                .Where(x => x.EventId == eventId)
                .ToListAsync();
        }

        public async Task AddAsync(IdeaSubmission idea)
        {
            _context.IdeaSubmissions.Add(idea);
            await _context.SaveChangesAsync();
        }
    }

}
