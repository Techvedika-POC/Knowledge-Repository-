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
    /// <summary>
    /// Service for computing topic highlights and retrieving knowledge items per topic/domain.
    /// </summary>
    public class TopicHighlightService : ITopicHighlightService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public TopicHighlightService(Knowledge_Repository_dbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves top highlighted topics/domains based on engagement and item count.
        /// </summary>
        /// <returns>List of TopicHighlightDto representing top topics</returns>
        public async Task<List<TopicHighlightDto>> GetTopicHighlightsAsync(int top = 10)
        {
            // Load all knowledge items including related domain, owner, and engagements
            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Owner)
                .Include(k => k.Engagements)
                .ToListAsync();

            // Group by domain name and compute topic metrics
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
                .OrderByDescending(t => t.EngagementScore) // prioritize by engagement
                .ThenByDescending(t => t.RecentItemCount)  // break ties by number of recent items
                .Take(top)
                .ToList();

            return topics;
        }

        /// <summary>
        /// Retrieves knowledge items belonging to a specific domain/topic.
        /// </summary>
        /// <returns>List of KnowledgeItemDto sorted by engagement and creation date</returns>
        public async Task<List<KnowledgeItemDto>> GetKnowledgeItemsByDomainAsync(string domainName, int top = 10)
        {
            // Fetch items for the given domain including necessary navigation properties
            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Engagements)
                .Where(k => k.Domain.DomainName == domainName)
                .ToListAsync();

            // Map to DTO, calculate engagement, and sort
            return items
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    ContributorName = k.Owner?.Name ?? "Unknown",
                    EngagementScore = k.Engagements.Count,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    Tags = k.KnowledgeTags.Select(t => t.TagName).ToList()
                })
                .OrderByDescending(k => k.EngagementScore)
                .ThenByDescending(k => k.CreatedOn)
                .Take(top)
                .ToList();
        }
    }
}
