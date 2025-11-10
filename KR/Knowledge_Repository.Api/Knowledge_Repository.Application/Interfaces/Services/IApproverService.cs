using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IApproverService
    {

        /// <summary>
        /// Retrieves all knowledge items that are pending approval.
        /// </summary>
        Task<List<KnowledgeItemDto>> GetPendingKnowledgeItemsAsync();
        /// <summary>
        /// Approves a specific knowledge item.
        /// </summary>
        Task<bool> ApproveKnowledgeItemAsync(Guid itemId, Guid approverId);
        /// <summary>
        /// Rejects a specific knowledge item.
        /// </summary>
        Task<bool> RejectKnowledgeItemAsync(Guid itemId, Guid approverId);
        /// <summary>
        /// Retrieves a paginated list of pending knowledge items.
        /// </summary>
        Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetPendingKnowledgeItemsAsync(int pageNumber, int pageSize);
    }
}
