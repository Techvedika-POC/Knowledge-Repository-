
﻿using KnowLedger_Synaptix.Models;

using System;

namespace KnowLedger_Synaptix.Dtos
{
    public class KnowledgeItemDto
    {
        // Unique identifier
        public Guid ItemId { get; set; }
        public int EngagementScore { get; set; }
        public DateTimeOffset CreatedOn { get; set; }


        // Main title of the knowledge item
        public string Title { get; set; }

        // Detailed description/content
        public string Description { get; set; }

        // Optional field (originally KnowledgeItem1 in EF Core, can rename appropriately)
        public string KnowledgeItem1 { get; set; }

        // Domain details
        public Guid? DomainId { get; set; }
        // From Domain navigation
        public string DomainName { get; set; } 

        // Category details
        public Guid? CategoryId { get; set; }
        // From Category navigation
        public string CategoryName { get; set; } 

        // Owner details
        public Guid? OwnerId { get; set; }
        // From Owner navigation
        public string OwnerName { get; set; } 

        // Status: Pending, Approved, Rejected
        public string Status { get; set; }

        // Version info
        public int? Version { get; set; }

        // Visibility
        public string Visibility { get; set; }

        // Is this an event-related item?
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

        public string ContributorName { get; set; }
       

        public User User { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;


        public List<string>? Tags { get; set; }= new List<string>();


    }
}
