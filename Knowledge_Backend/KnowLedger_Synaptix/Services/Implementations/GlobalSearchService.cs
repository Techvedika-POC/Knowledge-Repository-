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
    /// Service to perform global search across knowledge items, domains, categories, attachments, and tags.
    /// </summary>
    public class GlobalSearchService : IGlobalSearchService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public GlobalSearchService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs a keyword-based search across multiple related entities.
        /// </summary>
        /// <param name="keyword">Search keyword</param>
        /// <returns>List of GlobalSearchResultDto containing search results</returns>
        public async Task<List<GlobalSearchResultDto>> GlobalSearchAsync(string keyword)
        {
            // Return empty list if keyword is null or whitespace
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<GlobalSearchResultDto>();

            keyword = keyword.ToLower();

            // Search across KnowledgeItems, Domains, Categories, Attachments, and Tags
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
                            Id = k.ItemId,
                            Name = k.Title,
                            // Return a snippet of description, truncate if longer than 200 characters
                            Snippet = k.Description.Length > 200 ? k.Description.Substring(0, 200) + "..." : k.Description,
                            CreatedOn = k.CreatedOn ?? DateTime.MinValue
                        };

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

            // Order by creation date descending
            return results.OrderByDescending(r => r.CreatedOn).ToList();
        }
    }
}
