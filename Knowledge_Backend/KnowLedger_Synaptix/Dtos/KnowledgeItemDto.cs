using KnowLedger_Synaptix.Models;
using System;
using System.Collections.Generic;

namespace KnowLedger_Synaptix.Dtos
{
    public class KnowledgeItemDto
    {
        // Unique identifier
        public Guid ItemId { get; set; }
        public int EngagementScore { get; set; }
        public DateTime CreatedOn { get; set; }

        // Main title of the knowledge item
        public string Title { get; set; }

        // Detailed description/content
        public string Description { get; set; }

        // Optional field
        public string KnowledgeItem { get; set; }

        // Domain details
        public Guid? DomainId { get; set; }
        public string DomainName { get; set; }

        // Category details
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Owner details
        public Guid? OwnerId { get; set; }
        public string OwnerName { get; set; }

        // Status: Pending, Approved, Rejected
        public string Status { get; set; }

        // Version info
        public int? Version { get; set; }

        // Visibility
        public string Visibility { get; set; }

        // Event-related item flag
        public bool? IsEventItem { get; set; }

        public string ContributorName { get; set; }

        public Guid? CreatedBy { get; set; }
        public string CreatedByName { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public Guid? UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }

        // Programming info
        public string Framework { get; set; }
        public string Language { get; set; }

        // Metadata (JSON/string)
        public string Metadata { get; set; }

        public User User { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;
        public int Views { get; set; } = 0;
        public int Likes { get; set; } = 0;
        public int Comments { get; set; } = 0;
    }
}
