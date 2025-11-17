using Knowledge_Repository.Application.Dtos.EventInsight;

public class FeedbackResponseDto
{
    public Guid FeedbackId { get; set; }
    public Guid MentorId { get; set; }
    public Guid TeamId { get; set; }
    public string FeedbackText { get; set; }
    public int? ProgressRating { get; set; }
    public DateTime CreatedOn { get; set; }

    public List<FeedbackReplyDto> Replies { get; set; } = new();
}
