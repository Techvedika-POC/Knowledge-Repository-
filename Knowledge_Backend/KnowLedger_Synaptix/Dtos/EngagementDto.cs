namespace KnowLedger_Synaptix.Dtos
{
    public class EngagementDto
    {
        public Guid ItemId { get; set; }
        public Guid UserId { get; set; }
        public string EngagementType { get; set; } = string.Empty;
        public string? CommentText { get; set; }
    }
}
