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
    public class GlobalSearchService : IGlobalSearchService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public GlobalSearchService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<List<KnowledgeItemFilterDto>> GlobalSearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<KnowledgeItemFilterDto>();

            keyword = keyword.ToLower();

            var query = _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Where(k =>
                    k.Title.ToLower().Contains(keyword) ||
                    k.Description.ToLower().Contains(keyword) ||
                    (k.Domain != null && k.Domain.DomainName.ToLower().Contains(keyword)) ||
                    (k.Category != null && k.Category.CategoryName.ToLower().Contains(keyword)) ||
                    _context.KnowledgeTags.Any(t => t.ItemId == k.ItemId && t.TagName.ToLower().Contains(keyword)) ||
                    _context.Attachments.Any(a => a.ItemId == k.ItemId && a.FileName.ToLower().Contains(keyword))
                )
                .Select(k => new KnowledgeItemFilterDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description.Length > 200 ? k.Description.Substring(0, 200) + "..." : k.Description,
                    DomainName = k.Domain != null ? k.Domain.DomainName : "Unknown Domain",
                    CategoryName = k.Category != null ? k.Category.CategoryName : "Unknown Category",
                    SubmittedBy = k.Owner != null ? k.Owner.Name : "Unknown",
                    Status = k.Status ?? string.Empty,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    User = k.Owner,
                    Tags = _context.KnowledgeTags
                        .Where(t => t.ItemId == k.ItemId)
                        .Select(t => t.TagName)
                        .ToList()
                });

            var results = await query
                .OrderByDescending(k => k.CreatedOn)
                .ToListAsync(); // ✅ Removed .Distinct()

            // ✅ Perform distinct in-memory safely
            var distinctResults = results
                .GroupBy(k => k.ItemId)
                .Select(g => g.First())
                .ToList();

            return distinctResults;
        }
    }
}
