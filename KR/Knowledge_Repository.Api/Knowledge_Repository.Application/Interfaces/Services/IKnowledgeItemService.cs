using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    /// <summary>
    /// Handles all business logic related to Knowledge Items — creation, retrieval, filtering, and management.
    /// </summary>
    public interface IKnowledgeItemService
    {
        /// <summary>
        /// Uploads a new knowledge item created by a specific user.
        /// </summary>
        Task<KnowledgeItem> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId);

        /// <summary>
        /// Retrieves detailed information for a specific knowledge item.
        /// </summary>
        Task<KnowledgeItemDetailsDto?> GetKnowledgeItemDetailsAsync(Guid itemId);

        /// <summary>
        /// Retrieves knowledge items with optional filters for domain and category.
        /// </summary>
        Task<IEnumerable<KnowledgeItemDto>> GetKnowledgeItemsAsync(Guid? domainId = null, Guid? categoryId = null);

        /// <summary>
        /// Retrieves items submitted by a specific owner.
        /// </summary>
        Task<IEnumerable<KnowledgeItemDto>> GetKnowledgeItemsByOwnerAsync(Guid ownerId);

        /// <summary>
        /// Retrieves the latest N "Fresh Pick" knowledge items.
        /// </summary>
        Task<IEnumerable<KnowledgeItemDto>> GetFreshPicksAsync(int count = 10);
    }
}
