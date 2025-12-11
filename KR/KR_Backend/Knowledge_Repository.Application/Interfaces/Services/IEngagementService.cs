using Knowledge_Repository.Application.Dtos;

public interface IEngagementService
{
    Task AddEngagementAsync(EngagementDto dto);
    Task RemoveEngagementAsync(Guid itemId, Guid userId, string engagementType);
    Task<KnowledgeItemEngagementDto> GetEngagementSummaryAsync(Guid itemId, Guid userId);
    Task<List<UserEngagementDto>> GetUserEngagementsAsync(Guid userId);
    Task<int> GetLikesCountAsync(Guid itemId);

    Task<List<UserLeaderboardDto>> GetTopUsersByLikesAsync(int top = 3);

    Task<List<CommentDto>> GetCommentsByItemAsync(Guid itemId);
    Task<List<CommentDto>> GetRepliesAsync(Guid parentCommentId);
    Task AddCommentAsync(CommentDto dto);
    Task DeleteCommentAsync(Guid engagementId);
    Task UpdateCommentAsync(Guid engagementId, string newText);
}
