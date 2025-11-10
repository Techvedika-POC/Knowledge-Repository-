using Knowledge_Repository.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IReadOnlyKnowledgeRepository
    {
        Task<List<KnowledgeItem>> GetFreshPicksAsync(int count);
        Task<List<KnowledgeItem>> GetTrendingAsync(int top);
        Task<List<KnowledgeItem>> GlobalSearchAsync(string keyword);
        Task<List<Topic>> GetTopicHighlightsAsync(int top);
        Task<List<KnowledgeItem>> GetKnowledgeItemsByDomainAsync(string domainName, int top);
    }
}
