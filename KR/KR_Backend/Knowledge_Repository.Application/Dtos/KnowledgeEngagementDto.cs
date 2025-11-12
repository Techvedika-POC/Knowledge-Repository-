using Knowledge_Repository.Application.Dtos;
public class KnowledgeItemEngagementDto
{
    public Guid ItemId { get; set; }
    public int LikesCount { get; set; }
    public int FavouritesCount { get; set; }
    public int CommentsCount { get; set; }
    public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
    public List<string> UserEngagementTypes { get; set; } = new List<string>();
}