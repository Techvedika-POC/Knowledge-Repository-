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

        public async Task<List<KnowledgeItemDto>> GlobalSearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<KnowledgeItemDto>();

            keyword = keyword.ToLower();

            var query = _context.KnowledgeItems
                .Include(k => k.Owner)        // get SubmittedBy
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Attachments)
                .AsQueryable();

            // Filter by keyword
            query = query.Where(k =>
                k.Title.ToLower().Contains(keyword) ||
                k.Description.ToLower().Contains(keyword) ||
                k.KnowledgeTags.Any(t => t.TagName.ToLower().Contains(keyword)) ||
                k.Attachments.Any(a => a.FileName.ToLower().Contains(keyword)) ||
                (k.Domain != null && k.Domain.DomainName.ToLower().Contains(keyword)) ||
                (k.Category != null && k.Category.CategoryName.ToLower().Contains(keyword))
            );

            var results = await query
                .OrderByDescending(k => k.CreatedOn)
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    DomainId = k.DomainId,
                    DomainName = k.Domain != null ? k.Domain.DomainName : "",
                    CategoryId = k.CategoryId,
                    CategoryName = k.Category != null ? k.Category.CategoryName : "",
                    OwnerId = k.OwnerId,
                    SubmittedBy = k.Owner != null ? k.Owner.Name : "Unknown",
                    Status = k.Status,
                    Version = k.Version,
                    Visibility = k.Visibility,
                    IsEventItem = k.IsEventItem,

                    Framework = k.Framework,
                    Language = k.Language,
                    Tags = k.KnowledgeTags.Select(t => t.TagName).ToList(),
                })
                .ToListAsync();

            return results;
        }
    }
}
