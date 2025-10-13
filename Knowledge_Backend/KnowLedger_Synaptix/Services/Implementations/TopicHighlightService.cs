using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class TopicHighlightService : ITopicHighlightService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public TopicHighlightService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        // Implement interface
        public async Task<List<TopicHighlightDto>> GetTopicHighlightsAsync(int top = 10)
        {
            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Owner)
                .Include(k => k.Engagements)
                .ToListAsync();

            var topics = items
                .Where(k => k.Domain != null)
                .GroupBy(k => k.Domain.DomainName)
                .Select(g => new TopicHighlightDto
                {
                    TopicName = g.Key,
                    RecentItemCount = g.Count(),
                    ExampleContributors = g.Select(k => k.Owner?.Name ?? "Unknown")
                                           .Distinct()
                                           .Take(3)
                                           .ToArray(),
                    EngagementScore = g.Sum(k => k.Engagements.Count)
                })
                .OrderByDescending(t => t.EngagementScore)
                .ThenByDescending(t => t.RecentItemCount)
                .Take(top)
                .ToList();

            return topics;
        }

        // Implement interface
        public async Task<List<KnowledgeItemDto>> GetKnowledgeItemsByDomainAsync(string domainName, int top = 10)
        {
            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Owner)
                .Include(k => k.Engagements)
                .Where(k => k.Domain.DomainName == domainName)
                .ToListAsync();

            return items
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    ContributorName = k.Owner?.Name ?? "Unknown",
                    EngagementScore = k.Engagements.Count,
                    CreatedOn = k.CreatedOn ?? System.DateTime.MinValue
                })
                .OrderByDescending(k => k.EngagementScore)
                .ThenByDescending(k => k.CreatedOn)
                .Take(top)
                .ToList();
        }
    }
}
