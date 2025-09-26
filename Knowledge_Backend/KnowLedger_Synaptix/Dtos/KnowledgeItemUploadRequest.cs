namespace KnowLedger_Synaptix.Dtos
{
    public class KnowledgeItemUploadRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<FileAttachmentDto>? Attachments { get; set; }

        // Event-related fields
        public bool IsEventItem { get; set; }
        public Guid? EventId { get; set; }
        public string? TeamName { get; set; }
        public List<string>? TeamMemberEmails { get; set; }
    }
}
