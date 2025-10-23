using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class GlobalSearchService : IGlobalSearchService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public GlobalSearchService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }
        public async Task<List<KnowledgeItemDto>> GlobalSearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<KnowledgeItemDto>();

            keyword = keyword.ToLower();

            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Engagements)
                .Where(k =>
                    k.Title.ToLower().Contains(keyword) ||
                    k.Description.ToLower().Contains(keyword) ||
                    (k.Domain != null && k.Domain.DomainName.ToLower().Contains(keyword)) ||
                    (k.Category != null && k.Category.CategoryName.ToLower().Contains(keyword)) ||
                    k.KnowledgeTags.Any(t => t.TagName.ToLower().Contains(keyword)) ||
                    _context.Attachments.Any(a => a.ItemId == k.ItemId && a.FileName.ToLower().Contains(keyword))
                )
                .OrderByDescending(k => k.CreatedOn)
                .ToListAsync();

            // ✅ Map to DTO including all required fields
            return items.Select(k => new KnowledgeItemDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description?.Length > 200 ? k.Description.Substring(0, 200) + "..." : k.Description,
                DomainId = k.DomainId,
                DomainName = k.Domain?.DomainName ?? string.Empty,
                CategoryId = k.CategoryId,
                CategoryName = k.Category?.CategoryName ?? string.Empty,
                OwnerId = k.OwnerId,
                OwnerName = k.Owner?.Name ?? "Unknown",
                CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                Framework = k.Framework,       // JSON string (parsed in frontend)
                Language = k.Language,         // JSON string (parsed in frontend)
                Tags = k.KnowledgeTags.Select(t => t.TagName).ToList(),
                EngagementScore = k.Engagements.Count,
                Status = k.Status ?? "Draft",
                IsEventItem = k.IsEventItem ?? false
            }).ToList();
        }
    }
}
