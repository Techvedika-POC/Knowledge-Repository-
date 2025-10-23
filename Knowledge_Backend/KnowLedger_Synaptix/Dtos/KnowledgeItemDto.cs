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
        public DateTimeOffset CreatedOn { get; set; }
        public string Language { get; set; } = "[]";  
        public string Framework { get; set; } = "[]";  
        public int Views { get; set; }       
        public int Likes { get; set; }       
        public int Comments { get; set; }    
        public string Title { get; set; }
        public string Description { get; set; }

        // Optional field
        public string KnowledgeItem { get; set; }

        // Domain details
        public Guid? DomainId { get; set; }
        public string DomainName { get; set; }
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string Status { get; set; }
        public int? Version { get; set; }
        public string Visibility { get; set; }

        // Event-related
        public bool? IsEventItem { get; set; }
        public string ContributorName { get; set; }
        public Guid? CreatedBy { get; set; }
        public string CreatedByName { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public Guid? UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }
        public string Metadata { get; set; }

        public User User { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;

    }
}
