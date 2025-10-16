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

        /// <summary>
        /// Retrieves all knowledge items that belong to a specific domain.
        /// </summary>
        Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByDomainAsync(Guid domainId);

        /// <summary>
        /// Retrieves all knowledge items that belong to a specific category.
        /// </summary>
        Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByCategoryAsync(Guid categoryId);

        /// <summary>
        /// Retrieves all knowledge items available in the system.
        /// </summary>
        Task<IEnumerable<KnowledgeItemFilterDto>> GetAllKnowledgeItemsAsync();
    }
}
