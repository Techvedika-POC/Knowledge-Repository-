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
        ///Retrieves all knowledge items that belong to a specific domain.
        /// Retrieves all knowledge items that belong to a specific category.
        /// Retrieves knowledge items with optional filters for domain and category.
        /// If no filter is provided, returns all items.
        /// </summary>
        Task<IEnumerable<KnowledgeItemDto>> GetKnowledgeItemsAsync(Guid? domainId = null, Guid? categoryId = null);


    }
}
