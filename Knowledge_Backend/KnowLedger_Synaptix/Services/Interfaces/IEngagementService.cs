using KnowLedger_Synaptix.Dtos;

public interface IEngagementService
{
    Task AddEngagementAsync(EngagementDto dto);
    Task RemoveEngagementAsync(Guid itemId, Guid userId, string engagementType);
    Task<KnowledgeItemEngagementDto> GetEngagementSummaryAsync(Guid itemId, Guid userId);
    Task<List<UserEngagementDto>> GetUserEngagementsAsync(Guid userId);

    Task<int> GetLikesCountAsync(Guid itemId);
}
