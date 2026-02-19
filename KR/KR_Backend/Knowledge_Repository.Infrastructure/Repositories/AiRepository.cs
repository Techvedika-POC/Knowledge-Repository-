using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Knowledge_Repository.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class AiRepository : IAiRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public AiRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task SaveInsightAsync(AiInsight insight)
        {
            _context.AiInsights.Add(insight);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AiInsight>> GetInsightsAsync(Guid entityId)
        {
            return await _context.AiInsights
                .Where(x => x.EntityId == entityId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync();
        }

        public async Task SaveConversationAsync(AiConversation message)
        {
            _context.AiConversations.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AiConversation>> GetConversationAsync(Guid userId, Guid? eventId)
        {
            return await _context.AiConversations
                .Where(x => x.UserId == userId && x.EventId == eventId)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync();
        }
        public async Task<AiInsight?> GetLatestInsightAsync(string entityType, Guid teamId)
        {
            return await _context.AiInsights
                .Where(x =>
                    x.EntityType == entityType &&
                    x.TeamId == teamId)
                .OrderByDescending(x => x.CreatedOn)
                .FirstOrDefaultAsync();
        }

    }


}
