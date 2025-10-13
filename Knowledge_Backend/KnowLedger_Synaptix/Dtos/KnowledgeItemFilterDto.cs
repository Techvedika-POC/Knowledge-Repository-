namespace KnowLedger_Synaptix.Dtos
{
    public class KnowledgeItemFilterDto
    {
        // Title of the knowledge item
        public string Title { get; set; } = string.Empty;

        // Domain name (e.g., Artificial Intelligence, Cloud)
        public string DomainName { get; set; } = string.Empty;

        // Category name (e.g., Machine Learning, C#)
        public string CategoryName { get; set; } = string.Empty;

        // Detailed description/content
        public string Description { get; set; } = string.Empty;

        // Name of the user who submitted the item
        public string SubmittedBy { get; set; } = string.Empty;

        // Status of the item (Pending, Approved, Rejected)
        public string Status { get; set; } = string.Empty;

        // Date when the item was created/submitted
        public DateTime CreatedOn { get; set; } = default;

        // Optional: Date range filter - from and to (useful for frontend filtering)
        public DateTime? CreatedFrom { get; set; } = null;
        public DateTime? CreatedTo { get; set; } = null;
    }
}
