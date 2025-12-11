using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IApproverRepository
    {
        Task<int> GetTotalPendingAsync();
        Task<int> GetNormalPendingAsync();
        Task<int> GetEventPendingAsync();

        Task<List<(Guid EventId, string EventTitle, int Count)>> GetEventWisePendingAsync();

        Task<List<KnowledgeItem>> GetPendingNormalItemsAsync(int page, int size);
        Task<List<KnowledgeItem>> GetPendingEventItemsAsync(int page, int size);
        Task<List<KnowledgeItem>> GetPendingItemsByEventAsync(Guid eventId, int page, int size);

        Task<bool> ApproveAsync(Guid itemId, Guid approverId);
        Task<bool> RejectAsync(Guid itemId, Guid approverId);
        Task<IEnumerable<string>> GetEventTypesAsync();
        Task<List<KnowledgeItem>> GetPendingItemsByEventTypeAsync(string eventType, int page, int size);
        Task<int> GetPendingEventTypeCountAsync(string eventType);


    }
}
