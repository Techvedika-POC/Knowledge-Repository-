using Knowledge_Repository.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace   Knowledge_Repository.Application.Interfaces.Services
{
    public interface ITopicHighlightService
    {
 
        Task<List<TopicHighlightDto>> GetTopicHighlightsAsync(int top = 10);
        Task<List<KnowledgeItemDto>> GetKnowledgeItemsByDomainAsync(string domainName, int top = 10);
    }
}
