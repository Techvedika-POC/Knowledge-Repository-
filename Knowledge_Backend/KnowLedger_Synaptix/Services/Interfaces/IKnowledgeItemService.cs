using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using System;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IKnowledgeItemService
    {
        Task<KnowledgeItem> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId);

        Task<KnowledgeItemDetailsDto> GetKnowledgeItemDetailsAsync(Guid itemId);
        Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemSummariesAsync(
      string sortOrder = "desc",
      DateTime? filterDate = null
        );
        Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByDomainAsync(Guid domainId);
        Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByCategoryAsync(Guid categoryId);

        Task<IEnumerable<KnowledgeItemFilterDto>> GetAllKnowledgeItemsAsync();

    }
}
