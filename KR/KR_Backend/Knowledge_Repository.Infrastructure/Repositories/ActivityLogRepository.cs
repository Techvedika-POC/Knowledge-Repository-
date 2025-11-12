using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class ActivityLogRepository : GenericRepository<ActivityLog>, IActivityLogRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public ActivityLogRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ActivityLog>> GetUserContributionsAsync(Guid userId)
        {
            return await _context.ActivityLogs
                .Include(a => a.Item)
                    .ThenInclude(k => k.Domain)
                .Include(a => a.Item)
                    .ThenInclude(k => k.Category)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedOn)
                .ToListAsync();
        }

        public async Task<IEnumerable<ActivityLog>> GetUserContributionsFilteredAsync(
            Guid userId,
            string? domain,
            string? category,
            string? title,
            string? status,
            DateTime? date)
        {
            var query = _context.ActivityLogs
                .Include(a => a.Item)
                    .ThenInclude(k => k.Domain)
                .Include(a => a.Item)
                    .ThenInclude(k => k.Category)
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(domain))
                query = query.Where(a => a.Item.Domain.DomainName == domain);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(a => a.Item.Category.CategoryName == category);

            if (!string.IsNullOrEmpty(title))
                query = query.Where(a => a.Item.Title.Contains(title));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Action == status);

            if (date.HasValue)
                query = query.Where(a => a.CreatedOn.HasValue && a.CreatedOn.Value.Date == date.Value.Date);

            return await query
                .OrderByDescending(a => a.CreatedOn)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUserDomainsAsync(Guid userId)
        {
            return await _context.ActivityLogs
                .Include(a => a.Item)
                    .ThenInclude(k => k.Domain)
                .Where(a => a.UserId == userId)
                .Select(a => a.Item.Domain.DomainName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUserCategoriesAsync(Guid userId)
        {
            return await _context.ActivityLogs
                .Include(a => a.Item)
                    .ThenInclude(k => k.Category)
                .Where(a => a.UserId == userId)
                .Select(a => a.Item.Category.CategoryName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUserTitlesAsync(Guid userId)
        {
            return await _context.ActivityLogs
                .Include(a => a.Item)
                .Where(a => a.UserId == userId)
                .Select(a => a.Item.Title)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<ActivityLog>> GetUserContributionsThisMonthAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            return await _context.ActivityLogs
                .Include(a => a.Item)
                    .ThenInclude(k => k.Domain)
                .Include(a => a.Item)
                    .ThenInclude(k => k.Category)
                .Where(a => a.UserId == userId && a.CreatedOn >= firstDayOfMonth)
                .OrderByDescending(a => a.CreatedOn)
                .ToListAsync();
        }
    }
}
