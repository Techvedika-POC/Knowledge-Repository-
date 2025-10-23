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
        public List<string> Language { get; set; } = new ();
        public List<string> Framework{ get; set; } = new ();
        public List<string> Tags { get; set; } = new List<string>();
        public string Visibility { get; set; } = "Public";
        public List<FileAttachmentDto> Attachments { get; set; } = new List<FileAttachmentDto>();
        public bool IsEventItem { get; set; } = false;   
        public Guid? EventId { get; set; }              
        public List<string>? TeamMemberEmails { get; set; }   
        public string? TeamName { get; set; }
    }

   
}
