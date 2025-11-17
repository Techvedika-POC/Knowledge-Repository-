using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Dtos.Mentor;

public class TeamDetailsDto
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; }
    public Guid EventId { get; set; }
    public string Description { get; set; }
    public string ProjectTitle { get; set; }

    public List<MemberDto> Members { get; set; } = new();
    public List<FeedbackResponseDto> Feedbacks { get; set; } = new();
    public List<KnowledgeItemDto> Submissions { get; set; } = new();
}
