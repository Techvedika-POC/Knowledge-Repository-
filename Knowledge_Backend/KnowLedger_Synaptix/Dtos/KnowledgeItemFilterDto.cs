using KnowLedger_Synaptix.Models;
using System;
using System.Collections.Generic;

namespace KnowLedger_Synaptix.Dtos
{
    public class KnowledgeItemFilterDto
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DomainName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;

        // Status of the item (Pending, Approved, Rejected)
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = default;
        public DateTime? CreatedFrom { get; set; } = null;
        public DateTime? CreatedTo { get; set; } = null;
        public User User { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
