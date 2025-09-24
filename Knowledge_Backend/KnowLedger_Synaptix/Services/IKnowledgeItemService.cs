using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services
{
    public interface IKnowledgeItemService
    {
        Task<KnowledgeItemResponseDto> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId);

        Task<IEnumerable<KnowledgeItemResponseDto>> GetAllAsync();
        Task<KnowledgeItemResponseDto?> GetByIdAsync(Guid itemId);
        Task<KnowledgeItemResponseDto?> UpdateAsync(Guid itemId, KnowledgeItemUploadDto dto);
        Task<bool> DeleteAsync(Guid itemId);

        Task<IEnumerable<Domain>> GetAllDomainsAsync();
        Task<IEnumerable<Category>> GetCategoriesByDomainAsync(Guid domainId);
        Task<IEnumerable<Event>> GetAllEventsAsync();
    }
}
