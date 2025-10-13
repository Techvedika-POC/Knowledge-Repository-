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
    public class TrendingService : ITrendingService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public TrendingService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<List<TrendingDto>> GetTrendingAsync(int top = 5)
        {
            var items = await _context.KnowledgeItems
                .Include(k => k.Owner)
                .Include(k => k.Engagements)
                .Include(k => k.KnowledgeTags)
                .ToListAsync();

            var trending = items.Select(k => new TrendingDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                Views = k.Engagements.Count,
                Likes = k.Engagements.Count(e => e.Points.HasValue && e.Points > 0),
                Comments = k.Engagements.Count(e => !string.IsNullOrEmpty(e.CommentText)),
                ContributorName = k.Owner?.Name ?? "Unknown",
                ContributorAvatarUrl = "",
                Tags = k.KnowledgeTags.Select(t => t.TagName).ToArray()
            })
            .OrderByDescending(t => t.Views + t.Likes * 2 + t.Comments)
            .Take(top)
            .Select((t, idx) => { t.Rank = idx + 1; return t; })
            .ToList();

            return trending;
        }
    }
}
