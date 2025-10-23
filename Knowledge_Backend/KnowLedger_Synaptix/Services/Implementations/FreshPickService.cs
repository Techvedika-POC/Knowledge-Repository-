using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    /// <summary>
    /// Service to fetch the latest knowledge items ("Fresh Picks") for display.
    /// </summary>
    public class FreshPickService : IFreshPickService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public FreshPickService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the most recent knowledge items up to the specified count.
        /// Includes related Domain, Category, Owner, and Tags information.
        /// </summary>
        /// <param name="count">Maximum number of items to retrieve (default 10).</param>
        /// <returns>List of KnowledgeItemDto representing the fresh picks.</returns>
        public async Task<List<KnowledgeItemDto>> GetFreshPicksAsync(int count = 10)
        {
            // Fetch latest items
            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .OrderByDescending(k => k.CreatedOn)
                .Take(count)
                .ToListAsync();
            return items.Select(k => new KnowledgeItemDto
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
            }).ToList();
        }
    }
}
