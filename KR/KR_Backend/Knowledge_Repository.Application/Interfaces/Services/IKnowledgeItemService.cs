using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    
    public interface IKnowledgeItemService
    {
        Task<KnowledgeItem> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId);

        Task<KnowledgeItemDetailsDto?> GetKnowledgeItemDetailsAsync(Guid itemId);

        Task<IEnumerable<KnowledgeItemDto>> GetKnowledgeItemsAsync(Guid? domainId = null, Guid? categoryId = null);

        Task<IEnumerable<KnowledgeItemDto>> GetKnowledgeItemsByOwnerAsync(Guid ownerId);

        Task<IEnumerable<KnowledgeItemDto>> GetFreshPicksAsync(int count = 10);
    }
}
