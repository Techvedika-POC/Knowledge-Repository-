using KnowLedger_Synaptix.Dtos;

public class KnowledgeItemDetailsDto
{
    public Guid ItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContributorName { get; set; } = string.Empty;
    public int EngagementScore { get; set; }
    public DateTime? CreatedOn { get; set; }
    public List<string>? Tags { get; set; }
    public List<AttachmentDto>? Attachments { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
}
