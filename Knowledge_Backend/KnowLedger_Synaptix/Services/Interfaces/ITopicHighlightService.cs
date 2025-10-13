using KnowLedger_Synaptix.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface ITopicHighlightService
    {
 
        Task<List<TopicHighlightDto>> GetTopicHighlightsAsync(int top = 10);
        Task<List<KnowledgeItemDto>> GetKnowledgeItemsByDomainAsync(string domainName, int top = 10);
    }
}
