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
    /// Provides services to track and retrieve user activity logs, contributions,
    /// and related metadata such as domains, categories, and titles.
    /// </summary>
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
            // Query all knowledge items owned by the user, including category and domain
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
            // Query knowledge item by ID including category and domain details
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
        //based on the attribute we get the items uploaded by the user
        public async Task<IEnumerable<ActivityLogDto>> GetUserContributionsFilteredAsync(
            Guid userId,
            string domain = null,
            string category = null,
            string title = null,
            string status = null,
            DateTime? date = null)
        {
            // Start query for user-owned knowledge items including domain and category
            var query = _context.KnowledgeItems
                .Where(k => k.OwnerId == userId)
                .Include(k => k.Category)
                .Include(k => k.Domain)
                .AsQueryable();

            // Apply optional domain filter
            if (!string.IsNullOrEmpty(domain))
                query = query.Where(k => k.Domain.DomainName.ToLower().Contains(domain.ToLower()));

            // Apply optional category filter
            if (!string.IsNullOrEmpty(category))
                query = query.Where(k => k.Category.CategoryName.ToLower().Contains(category.ToLower()));

            // Apply optional title filter
            if (!string.IsNullOrEmpty(title))
                query = query.Where(k => k.Title.ToLower().Contains(title.ToLower()));

            // Apply optional status filter
            if (!string.IsNullOrEmpty(status))
                query = query.Where(k => k.Status.ToLower().Contains(status.ToLower()));

            // Apply optional date filter (only items created on that specific day)
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
        //user uplaoded domains
        public async Task<IEnumerable<string>> GetUserDomainsAsync(Guid userId)
        {
            return await _context.KnowledgeItems
                .Where(k => k.OwnerId == userId && k.Domain != null)
                .Select(k => k.Domain.DomainName)
                .Distinct()
                .ToListAsync();
        }
        //user uplaoded categories
        public async Task<IEnumerable<string>> GetUserCategoriesAsync(Guid userId)
        {
            return await _context.KnowledgeItems
                .Where(k => k.OwnerId == userId && k.Category != null)
                .Select(k => k.Category.CategoryName)
                .Distinct()
                .ToListAsync();
        }
        //user uploade titles

        /// <summary>
        /// Retrieve all unique titles for a specific user.
        /// </summary>
        public async Task<IEnumerable<string>> GetUserTitlesAsync(Guid userId)
        {
            return await _context.KnowledgeItems
                .Where(k => k.OwnerId == userId)
                .Select(k => k.Title)
                .Distinct()
                .ToListAsync();
        }
        //pagination 
        public async Task<PagedResult<ActivityLogDto>> GetUserContributionsPagedAsync(
          Guid userId,
          int pageNumber = 1,
          int pageSize = 10,
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

            // Apply filters
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
                query = query.Where(k => k.CreatedOn >= selectedDate && k.CreatedOn < selectedDate.AddDays(1));
            }

          
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderByDescending(k => k.CreatedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            return new PagedResult<ActivityLogDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
        //items uplaoded by current month
        public async Task<IEnumerable<ActivityLogDto>> GetUserContributionsThisMonthAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayUtc = DateTime.SpecifyKind(firstDayOfMonth, DateTimeKind.Utc);

            return await _context.KnowledgeItems
                .Where(k => k.OwnerId == userId && k.CreatedOn >= firstDayUtc)
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
                    Status = k.Status,
                    Date = k.CreatedOn
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<KnowledgeItemDto>> GetUserFavouritesAsync(Guid userId)
        {
            var favouriteItems = await _context.KnowledgeItems
                .Include(k => k.Category)
                .Include(k => k.Domain)
                .Include(k => k.Owner)
                .Include(k => k.Engagements)
                .Where(k => k.Engagements.Any(e => e.UserId == userId && e.EngagementType == "Favourite"))
                .OrderByDescending(k => k.CreatedOn)
                .ToListAsync();

            return favouriteItems.Select(k => new KnowledgeItemDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                DomainId = k.DomainId,
                DomainName = k.Domain?.DomainName,
                CategoryId = k.CategoryId,
                CategoryName = k.Category?.CategoryName,
                OwnerId = k.OwnerId,
                OwnerName = k.Owner?.Name,
                Status = k.Status,
                CreatedOn = k.CreatedOn ?? DateTime.Now,
                UpdatedOn = k.UpdatedOn,
                Tags = k.KnowledgeTags.Select(t => t.TagName).ToList(),
                Language = k.Language,
                Framework = k.Framework,
                Views = k.Engagements.Count(e => e.EngagementType == "View"),
                Likes = k.Engagements.Count(e => e.EngagementType == "Like"),
                Comments = k.Engagements.Count(e => e.EngagementType == "Comment"),
                SubmittedBy = k.Owner?.Name
            }).ToList();
        }

    }
}
