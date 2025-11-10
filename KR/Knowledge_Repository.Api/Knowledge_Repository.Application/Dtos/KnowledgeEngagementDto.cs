
public class KnowledgeItemEngagementDto
{
    public Guid ItemId { get; set; }
    public int LikesCount { get; set; }
    public int FavouritesCount { get; set; }
    public int CommentsCount { get; set; }
    public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
    public List<string> UserEngagementTypes { get; set; } = new List<string>();
}
public class CommentDto
{
    public Guid EngagementId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string CommentText { get; set; }
    public DateTime CreatedOn { get; set; }
}
