using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IAiRepository
    {
        Task SaveInsightAsync(AiInsight insight);
        Task<List<AiInsight>> GetInsightsAsync(Guid entityId);

        Task SaveConversationAsync(AiConversation message);
        Task<List<AiConversation>> GetConversationAsync(Guid userId, Guid? eventId);
        Task<AiInsight?> GetLatestInsightAsync(string entityType, Guid teamId);
    }

}
