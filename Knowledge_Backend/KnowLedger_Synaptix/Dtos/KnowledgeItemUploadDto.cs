using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace KnowLedger_Synaptix.Dtos
{
    public class KnowledgeItemUploadDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid DomainId { get; set; }
        public Guid CategoryId { get; set; }
        public List<string> Languages { get; set; } = new List<string>();
        public List<string> Frameworks { get; set; } = new List<string>();
        // Tags
        public List<string> Tags { get; set; } = new List<string>();

        // Visibility setting
        public string Visibility { get; set; } = "Public";

        // Attachments
        public List<FileAttachmentDto> Attachments { get; set; } = new List<FileAttachmentDto>();

        // Event-related
        public bool IsEventItem { get; set; } = false;   // If true, link with Event
        public Guid? EventId { get; set; }               // Event ID
        public List<string>? TeamMemberEmails { get; set; }   // Use emails instead of IDs
        public string? TeamName { get; set; }
    }

   
}
