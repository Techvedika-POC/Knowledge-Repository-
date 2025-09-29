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

        public async Task<List<GlobalSearchResultDto>> GlobalSearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<GlobalSearchResultDto>();

            keyword = keyword.ToLower();

            // Unified search: KnowledgeItems, Domains, Categories, Attachments, Tags
            var query = from k in _context.KnowledgeItems
                        join d in _context.Domains on k.DomainId equals d.DomainId into kd
                        from domain in kd.DefaultIfEmpty()
                        join c in _context.Categories on k.CategoryId equals c.CategoryId into kc
                        from category in kc.DefaultIfEmpty()
                        where k.Title.ToLower().Contains(keyword)
                              || k.Description.ToLower().Contains(keyword)
                              || _context.KnowledgeTags.Any(t => t.ItemId == k.ItemId && t.TagName.ToLower().Contains(keyword))
                              || _context.Attachments.Any(a => a.ItemId == k.ItemId && a.FileName.ToLower().Contains(keyword))
                              || (domain != null && domain.DomainName.ToLower().Contains(keyword))
                              || (category != null && category.CategoryName.ToLower().Contains(keyword))
                        select new GlobalSearchResultDto
                        {
                            //Type = "KnowledgeItem",
                            Id = k.ItemId,
                            Name = k.Title,
                            Snippet = k.Description.Length > 200 ? k.Description.Substring(0, 200) + "..." : k.Description,
                        };

            var results = await query.Distinct().ToListAsync();

            return results.OrderByDescending(r => r.CreatedOn).ToList();
        }
    }
}
