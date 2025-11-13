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
    public class KnowledgeItemRepository : GenericRepository<KnowledgeItem>, IKnowledgeItemRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public KnowledgeItemRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<KnowledgeItem>> GetPendingItemsAsync(int pageNumber, int pageSize)
        {
            return await _context.KnowledgeItems
                .Where(k => k.Status == "Pending")
                .OrderBy(k => k.CreatedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .ToListAsync();
        }

        public async Task<int> GetPendingItemsCountAsync()
        {
            return await _context.KnowledgeItems
                .CountAsync(k => k.Status == "Pending");
        }

        public async Task<bool> ApproveItemAsync(Guid itemId, Guid approverId)
        {
            var item = await _context.KnowledgeItems.FindAsync(itemId);
            if (item == null) return false;

            item.Status = "Approved";
            item.UpdatedBy = approverId;
            item.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectItemAsync(Guid itemId, Guid approverId)
        {
            var item = await _context.KnowledgeItems.FindAsync(itemId);
            if (item == null) return false;

            item.Status = "Rejected";
            item.UpdatedBy = approverId;
            item.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<KnowledgeItem>> GetByDomainOrCategoryAsync(Guid? domainId, Guid? categoryId)
        {
            var query = _context.KnowledgeItems.AsQueryable();

            if (domainId.HasValue)
                query = query.Where(k => k.DomainId == domainId.Value);

            if (categoryId.HasValue)
                query = query.Where(k => k.CategoryId == categoryId.Value);

            return await query
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .ToListAsync();
        }
        public async Task<IEnumerable<KnowledgeItem>> GetByDomainNameAsync(string domainName)
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Engagements)
                .Where(k => k.Domain != null && k.Domain.DomainName == domainName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<KnowledgeItem>> GetAllWithDomainAndEngagementsAsync()
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.Engagements)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<KnowledgeItem>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<KnowledgeItem>();

            keyword = keyword.Trim().ToLower();

            var query = _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Engagements)
                .Where(k =>
                    (k.Title != null && k.Title.ToLower().Contains(keyword)) ||
                    (k.Description != null && k.Description.ToLower().Contains(keyword)) ||
                    (k.Domain != null && k.Domain.DomainName.ToLower().Contains(keyword)) ||
                    (k.Category != null && k.Category.CategoryName.ToLower().Contains(keyword)) ||
                    k.KnowledgeTags.Any(t => t.TagName.ToLower().Contains(keyword)) ||
                    _context.Attachments.Any(a => a.ItemId == k.ItemId && a.FileName.ToLower().Contains(keyword))
                )
                .OrderByDescending(k => k.CreatedOn)
                .AsNoTracking();

            return await query.ToListAsync();
        }
        public async Task<KnowledgeItem?> GetByIdWithDetailsAsync(Guid itemId)
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Attachments)
                .Include(k => k.Engagements)
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.ItemId == itemId);
        }

        public async Task<IEnumerable<KnowledgeItem>> GetByOwnerAsync(Guid ownerId)
        {
            return await _context.KnowledgeItems
                .Where(k => k.OwnerId == ownerId)
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Engagements)
                .OrderByDescending(k => k.CreatedOn)
                .ToListAsync();
        }
        public async Task<List<KnowledgeItem>> GetFreshPicksAsync(int count = 10)
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Engagements)
                .Where(k => k.Status == "Approved") 
                .OrderByDescending(k => k.CreatedOn)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<KnowledgeItem>> GetByItemIdsAsync(IEnumerable<Guid> itemIds)
        {
            return await _context.KnowledgeItems
                .Where(k => itemIds.Contains(k.ItemId))
                .Include(k => k.Owner) 
                .ToListAsync();
        }
        public async Task<KnowledgeItem?> GetByTitleAndUserAsync(string title, Guid userId)
        {
            return await _context.KnowledgeItems
                .Where(k => k.Title == title && k.OwnerId == userId)
                .FirstOrDefaultAsync();
        }

    }
}
