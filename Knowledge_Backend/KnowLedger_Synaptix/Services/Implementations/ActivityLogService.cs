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
    public class ActivityLogService : IActivityLogService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public ActivityLogService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        // Fetch all contributions of the logged-in user
        public async Task<IEnumerable<ActivityLogDto>> GetUserContributionsAsync(Guid userId)
        {
            var contributions = await _context.KnowledgeItems
                .Where(k => k.OwnerId == userId)
                .Include(k => k.Category)
                .Include(k => k.Domain)
                .OrderByDescending(k => k.CreatedOn)
                .Select(k => new ActivityLogDto
                {
                    UserId = k.OwnerId,
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Category = k.Category != null ? k.Category.CategoryName : null,
                    Domain = k.Domain != null ? k.Domain.DomainName : null,
                    Description = k.Description.Length > 100 ? k.Description.Substring(0, 100) + "..." : k.Description,
                    Status = k.Status, // Approved / Pending / Rejected
                    Date = k.CreatedOn
                })
                .ToListAsync();

            return contributions;
        }
        // Fetch full details for preview
        public async Task<ActivityLogDto> GetContributionDetailsAsync(Guid itemId)
        {
            var item = await _context.KnowledgeItems
                .Include(k => k.Category)
                .Include(k => k.Domain)
                .Where(k => k.ItemId == itemId)
                .Select(k => new ActivityLogDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Category = k.Category != null ? k.Category.CategoryName : null,
                    Domain = k.Domain != null ? k.Domain.DomainName : null,
                    Description = k.Description, 
                    Status = k.Status,
                    Date = k.CreatedOn
                })
                .FirstOrDefaultAsync();

            return item;
        }
        public async Task<IEnumerable<ActivityLogDto>> GetUserContributionsFilteredAsync(
            Guid userId,
            string domain = null,
            string category = null,
            string title = null,
            string status = null,
            DateTime? date = null)
        {
            var query = _context.KnowledgeItems
                .Where(k => k.OwnerId == userId)
                .Include(k => k.Category)
                .Include(k => k.Domain)
                .AsQueryable();

            if (!string.IsNullOrEmpty(domain))
                query = query.Where(k => k.Domain.DomainName.ToLower().Contains(domain.ToLower()));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(k => k.Category.CategoryName.ToLower().Contains(category.ToLower()));

            if (!string.IsNullOrEmpty(title))
                query = query.Where(k => k.Title.ToLower().Contains(title.ToLower()));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(k => k.Status.ToLower().Contains(status.ToLower()));

            if (date.HasValue)
            {
               
                var selectedDate = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);

                query = query.Where(k =>
                    k.CreatedOn >= selectedDate &&
                    k.CreatedOn < selectedDate.AddDays(1));
            }


            return await query
                .OrderByDescending(k => k.CreatedOn)
                .Select(k => new ActivityLogDto
                {
                    UserId = k.OwnerId,
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Category = k.Category != null ? k.Category.CategoryName : null,
                    Domain = k.Domain != null ? k.Domain.DomainName : null,
                    Description = k.Description.Length > 100 ? k.Description.Substring(0, 100) + "..." : k.Description,
                    Status = k.Status,
                    Date = k.CreatedOn
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUserDomainsAsync(Guid userId)
        {
            return await _context.KnowledgeItems
                .Where(k => k.OwnerId == userId && k.Domain != null)
                .Select(k => k.Domain.DomainName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUserCategoriesAsync(Guid userId)
        {
            return await _context.KnowledgeItems
                .Where(k => k.OwnerId == userId && k.Category != null)
                .Select(k => k.Category.CategoryName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUserTitlesAsync(Guid userId)
        {
            return await _context.KnowledgeItems
                .Where(k => k.OwnerId == userId)
                .Select(k => k.Title)
                .Distinct()
                .ToListAsync();
        }
    }
}
