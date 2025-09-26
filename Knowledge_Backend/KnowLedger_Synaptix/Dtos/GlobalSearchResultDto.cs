namespace KnowLedger_Synaptix.Dtos
{
    public class GlobalSearchResultDto
    {
        public string Type { get; set; }      // "KnowledgeItem" or "Event"
        public Guid Id { get; set; }          // ItemId or EventId
        public string Name { get; set; }      // Title/Name
        public string Snippet { get; set; }   // Short preview containing keyword
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } // Optional: user name
        public string EventType { get; set; } = string.Empty;
    }
}
