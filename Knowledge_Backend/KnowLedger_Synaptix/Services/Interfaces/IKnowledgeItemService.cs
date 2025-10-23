using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using System;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IKnowledgeItemService
    {
        /// <summary>
        /// Uploads a new knowledge item created by a specific user.
        /// </summary>

        Task<KnowledgeItem> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId);

        /// <summary>
        /// Retrieves detailed information for a specific knowledge item.
        /// </summary>
        Task<KnowledgeItemDetailsDto> GetKnowledgeItemDetailsAsync(Guid itemId);


        /// <summary>
        /// Retrieves a summarized list of knowledge items, optionally filtered by date and sorted by creation order.
        /// </summary>

        Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemSummariesAsync(
      string sortOrder = "desc",
      DateTime? filterDate = null
        );

        Task<List<KnowledgeItemDto>> GetAllKnowledgeItemsAsync();
        Task<List<KnowledgeItemDto>> GetKnowledgeItemsByDomainAsync(Guid domainId);
        Task<List<KnowledgeItemDto>> GetKnowledgeItemsByCategoryAsync(Guid categoryId);


    }
}
