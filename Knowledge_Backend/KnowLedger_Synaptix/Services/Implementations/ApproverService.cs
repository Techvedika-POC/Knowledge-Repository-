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
    /// Provides methods for approvers to fetch pending knowledge items 
    /// and approve or reject them.
    /// </summary>
    public class ApproverService : IApproverService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public ApproverService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }
        // Fetch pending items for approver
        public async Task<List<KnowledgeItemDto>> GetPendingKnowledgeItemsAsync()
        {
            // Query for pending knowledge items 
            var query = _context.KnowledgeItems
                .AsNoTracking()
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.CreatedByNavigation)
                .Where(k => k.Status == "Pending")
                .AsQueryable();
            return await query
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    DomainId = k.DomainId,
                    DomainName = k.Domain != null ? k.Domain.DomainName : string.Empty,
                    CategoryId = k.CategoryId,
                    CategoryName = k.Category != null ? k.Category.CategoryName : string.Empty,
                    OwnerId = k.OwnerId,
                    OwnerName = k.Owner != null ? k.Owner.Name : string.Empty,
                    Status = k.Status,
                    IsEventItem = k.IsEventItem,
                    Framework = k.Framework,
                    Language = k.Language,
                    Metadata = k.Metadata,
                    CreatedBy = k.CreatedBy,
                    CreatedByName = k.CreatedByNavigation != null ? k.CreatedByNavigation.Name : string.Empty,
                    UpdatedOn = k.UpdatedOn,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedByName = k.UpdatedByNavigation != null ? k.UpdatedByNavigation.Name : string.Empty
                })
                .ToListAsync();
        }
        //Approval 
        public async Task<bool> ApproveKnowledgeItemAsync(Guid itemId, Guid approverId)
        {
            // Find the item
            var item = await _context.KnowledgeItems.FindAsync(itemId);

            // Only pending items can be approved
            if (item == null || item.Status != "Pending") return false;

            // Update status and audit fields
            item.Status = "Approved";
            item.UpdatedBy = approverId;
            item.UpdatedOn = DateTime.UtcNow;

            _context.KnowledgeItems.Update(item);
            await _context.SaveChangesAsync();
            return true;
        }
        //reject
        public async Task<bool> RejectKnowledgeItemAsync(Guid itemId, Guid approverId)
        {
            // Find the item
            var item = await _context.KnowledgeItems.FindAsync(itemId);

            // Only pending items can be rejected
            if (item == null || item.Status != "Pending") return false;

            // Update status and audit fields
            item.Status = "Rejected";
            item.UpdatedBy = approverId;
            item.UpdatedOn = DateTime.UtcNow;

            _context.KnowledgeItems.Update(item);
            await _context.SaveChangesAsync();
            return true;
        }
        //pagination
        public async Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetPendingKnowledgeItemsAsync(int pageNumber, int pageSize)
        {
            var query = _context.KnowledgeItems
                .AsNoTracking()
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.CreatedByNavigation)
                .Where(k => k.Status == "Pending")
                .AsQueryable();

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(k => k.CreatedOn) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    DomainId = k.DomainId,
                    DomainName = k.Domain != null ? k.Domain.DomainName : string.Empty,
                    CategoryId = k.CategoryId,
                    CategoryName = k.Category != null ? k.Category.CategoryName : string.Empty,
                    OwnerId = k.OwnerId,
                    OwnerName = k.Owner != null ? k.Owner.Name : string.Empty,
                    Status = k.Status,
                    IsEventItem = k.IsEventItem,
                    Framework = k.Framework,
                    Language = k.Language,
                    Metadata = k.Metadata,
                    
                    CreatedBy = k.CreatedBy,
                    CreatedByName = k.CreatedByNavigation != null ? k.CreatedByNavigation.Name : string.Empty,
                    UpdatedOn = k.UpdatedOn,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedByName = k.UpdatedByNavigation != null ? k.UpdatedByNavigation.Name : string.Empty
                })
                .ToListAsync();

            return (items, totalCount);
        }

    }
}
