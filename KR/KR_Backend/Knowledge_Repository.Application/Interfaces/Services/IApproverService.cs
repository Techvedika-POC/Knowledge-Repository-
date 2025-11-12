using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IApproverService
    {

     
        Task<List<KnowledgeItemDto>> GetPendingKnowledgeItemsAsync();
     
        Task<bool> ApproveKnowledgeItemAsync(Guid itemId, Guid approverId);
     
        Task<bool> RejectKnowledgeItemAsync(Guid itemId, Guid approverId);
   
        Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetPendingKnowledgeItemsAsync(int pageNumber, int pageSize);
    }
}
