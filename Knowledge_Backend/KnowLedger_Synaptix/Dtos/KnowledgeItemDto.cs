namespace KnowLedger_Synaptix.Dtos
{
    public class KnowledgeItemDto
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContributorName { get; set; }
        public int EngagementScore { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
