using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IKnowledgeItemRepository : IGenericRepository<KnowledgeItem>
    {
        Task<List<KnowledgeItem>> GetPendingItemsAsync(int pageNumber, int pageSize);
        Task<int> GetPendingItemsCountAsync();
        Task<bool> ApproveItemAsync(Guid itemId, Guid approverId);
        Task<bool> RejectItemAsync(Guid itemId, Guid approverId);
        Task<IEnumerable<KnowledgeItem>> GetByDomainOrCategoryAsync(Guid? domainId, Guid? categoryId);
        Task<IEnumerable<KnowledgeItem>> GetByOwnerAsync(Guid ownerId);
        Task<List<KnowledgeItem>> GetFreshPicksAsync(int count = 10);
        Task<IEnumerable<KnowledgeItem>> SearchAsync(string keyword);

        Task<KnowledgeItem?> GetByIdWithDetailsAsync(Guid itemId);
        Task<IEnumerable<KnowledgeItem>> GetByDomainNameAsync(string domainName);
        Task<IEnumerable<KnowledgeItem>> GetAllWithDomainAndEngagementsAsync();
        Task<List<KnowledgeItem>> GetByItemIdsAsync(IEnumerable<Guid> itemIds);
        Task<KnowledgeItem?> GetByTitleAndUserAsync(string title, Guid userId);
        Task<List<KnowledgeVersion>> GetVersionsWithAttachmentsAsync(Guid itemId, bool onlyLatest = false);
        Task<List<KnowledgeItem>> GetApprovedEventItemsAsync();

    }
}