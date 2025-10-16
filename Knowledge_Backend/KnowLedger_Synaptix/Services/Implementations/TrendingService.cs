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
    /// Service for retrieving trending knowledge items based on engagement.
    /// </summary>
    public class TrendingService : ITrendingService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public TrendingService(Knowledge_Repository_dbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets top trending knowledge items, ranked by engagement score.
        /// </summary>
        /// <returns>List of KnowledgeItemDto representing trending items</returns>
        public async Task<List<KnowledgeItemDto>> GetTrendingAsync(int top = 5)
        {
            // Load all knowledge items including related owner, tags, and engagements
            var items = await _context.KnowledgeItems
                .Include(k => k.Owner)
                .Include(k => k.Engagements)
                .Include(k => k.KnowledgeTags)
                .ToListAsync();

            // Map each item to DTO and compute engagement score
            var trending = items
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    ContributorName = k.Owner?.Name ?? "Unknown",
                    EngagementScore = k.Engagements.Count, // Total likes, favorites, etc.
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    Tags = k.KnowledgeTags.Select(t => t.TagName).ToList()
                })
                .OrderByDescending(k => k.EngagementScore) // Rank by engagement
                .Take(top) // Only return the top N items
                .ToList();

            return trending;
        }
    }
}
