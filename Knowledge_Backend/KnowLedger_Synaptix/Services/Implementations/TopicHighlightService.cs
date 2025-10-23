using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ------------------ Top Topics ------------------
        public async Task<List<TopicHighlightDto>> GetTopicHighlightsAsync(int top = 10)
        {
            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Owner)
                .Include(k => k.Category)
                .Include(k => k.Engagements)
                .ToListAsync();

            var topics = items
                .Where(k => k.Domain != null)
                .GroupBy(k => k.Domain.DomainName)
                .Select(g => new TopicHighlightDto
                {
                    TopicName = g.Key,
                    RecentItemCount = g.Count(),
                    ExampleContributors = g
                        .Select(k => k.Owner?.Name ?? "Unknown")
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

        // ------------------ Knowledge Items By Domain ------------------
        public async Task<List<KnowledgeItemDto>> GetKnowledgeItemsByDomainAsync(string domainName, int top = 10)
        {
            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Engagements)
                .Where(k => k.Domain.DomainName == domainName)
                .ToListAsync();

            return items
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    DomainId = k.DomainId,
                    DomainName = k.Domain?.DomainName,
                    CategoryId = k.CategoryId,
                    CategoryName = k.Category?.CategoryName,
                    Language = k.Language,
                    Framework = k.Framework,
                    OwnerId = k.OwnerId,
                    OwnerName = k.Owner?.Name ?? "Unknown",
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    Tags = k.KnowledgeTags.Select(t => t.TagName).ToList(),
                    EngagementScore = k.Engagements.Count
                })
                .OrderByDescending(k => k.EngagementScore)
                .ThenByDescending(k => k.CreatedOn)
                .Take(top)
                .ToList();
        }
    }
}
