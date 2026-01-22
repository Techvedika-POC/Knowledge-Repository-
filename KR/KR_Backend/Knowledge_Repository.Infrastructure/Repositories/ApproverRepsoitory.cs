using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class ApproverRepository : IApproverRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public ApproverRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalPendingAsync() =>
            await _context.KnowledgeItems.CountAsync(k => k.Status == "Pending");

        public async Task<int> GetNormalPendingAsync() =>
            await _context.KnowledgeItems.CountAsync(k => k.Status == "Pending" && !k.IsEventItem.Value);

        public async Task<int> GetEventPendingAsync() =>
            await _context.KnowledgeItems.CountAsync(k => k.Status == "Pending" && k.IsEventItem.Value);

        public async Task<List<(Guid EventId, string EventTitle, int Count)>> GetEventWisePendingAsync()
        {
            return await _context.EventKnowledgeItems
                .Where(e => e.Item.Status == "Pending")
                .GroupBy(e => new { e.EventId, e.Event.Title })
                .Select(g => new ValueTuple<Guid, string, int>(
                    g.Key.EventId.Value,
                    g.Key.Title,
                    g.Count()
                ))
                .ToListAsync();
        }

        public async Task<List<KnowledgeItem>> GetPendingNormalItemsAsync(int page, int size)
        {
            return await _context.KnowledgeItems
                .Where(k => k.Status == "Pending" && !k.IsEventItem.Value)
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<List<KnowledgeItem>> GetPendingEventItemsAsync(int page, int size)
        {
            return await _context.KnowledgeItems
                .Where(k => k.Status == "Pending" && k.IsEventItem.Value)
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<List<KnowledgeItem>> GetPendingItemsByEventAsync(Guid eventId, int page, int size)
        {
            return await _context.EventKnowledgeItems
                .Where(e => e.EventId == eventId && e.Item.Status == "Pending")
                .Select(e => e.Item)
                .Include(i => i.Domain)
                .Include(i => i.Category)
                .Include(i => i.Owner)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<bool> ApproveAsync(Guid itemId, Guid approverId)
        {
            var item = await _context.KnowledgeItems.FindAsync(itemId);
            if (item == null) return false;

            item.Status = "Approved";
            item.UpdatedBy = approverId;
            item.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(Guid itemId, Guid approverId, string feedback)
        {
            // 1. Direct SQL update (NO tracking, NO concurrency)
            var rows = await _context.KnowledgeItems
                .Where(x => x.ItemId == itemId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, "Rejected")
                    .SetProperty(x => x.UpdatedBy, approverId)
                    .SetProperty(x => x.UpdatedOn, DateTime.UtcNow)
                );

            if (rows == 0)
                return false;   // item not found or already gone

            // 2. Insert review/feedback
            var review = new KnowledgeReview
            {
                ReviewId = Guid.NewGuid(),
                ItemId = itemId,
                ReviewerId = approverId,
                Decision = "Rejected",
                Comments = feedback,
                ReviewedOn = DateTime.UtcNow
            };

            _context.KnowledgeReviews.Add(review);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<IEnumerable<string>> GetEventTypesAsync()
        {
            return await _context.Events
                .Where(e => !string.IsNullOrEmpty(e.EventType))
                .Select(e => e.EventType)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
        }
        public async Task<List<KnowledgeItem>> GetPendingItemsByEventTypeAsync(
         string eventType, int page, int size)
        {
            return await _context.EventKnowledgeItems
                .Where(e => e.Event.EventType == eventType && e.Item.Status == "Pending")
                .Include(e => e.Item.Domain)
                .Include(e => e.Item.Category)
                .Include(e => e.Item.Owner)
                .Select(e => e.Item)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }


        public async Task<int> GetPendingEventTypeCountAsync(string eventType)
        {
            return await _context.EventKnowledgeItems
                .Where(e => e.Event.EventType == eventType && e.Item.Status == "Pending")
                .CountAsync();
        }

    }
}
