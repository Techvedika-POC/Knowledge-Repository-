using System;

namespace KnowLedger_Synaptix.DTOs
{
    public class KnowledgeItemResponseDto
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? DomainId { get; set; }
        public Guid? CategoryId { get; set; }
        public string? Language { get; set; }
        public string? Framework { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
