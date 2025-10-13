using KnowLedger_Synaptix.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IApproverService
    {
        Task<List<KnowledgeItemDto>> GetPendingKnowledgeItemsAsync();
        Task<bool> ApproveKnowledgeItemAsync(Guid itemId, Guid approverId);
        Task<bool> RejectKnowledgeItemAsync(Guid itemId, Guid approverId);
    }
}
