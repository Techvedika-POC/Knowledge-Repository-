
using System;

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

        // Optional field (originally KnowledgeItem1 in EF Core, can rename appropriately)
        public string KnowledgeItem1 { get; set; }

        // Domain details
        public Guid? DomainId { get; set; }
        public string DomainName { get; set; } // From Domain navigation

        // Category details
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; } // From Category navigation

        // Owner details
        public Guid? OwnerId { get; set; }
        public string OwnerName { get; set; } // From Owner navigation

        // Status: Pending, Approved, Rejected
        public string Status { get; set; }

        // Version info
        public int? Version { get; set; }

        // Visibility
        public string Visibility { get; set; }

        // Is this an event-related item?
        public bool? IsEventItem { get; set; }

        public Guid? CreatedBy { get; set; }
        public string CreatedByName { get; set; } // From CreatedByNavigation

        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string UpdatedByName { get; set; } // From UpdatedByNavigation

        // Programming info
        public string Framework { get; set; }
        public string Language { get; set; }

        // Metadata (JSON/string)
        public string Metadata { get; set; }
    }
}
