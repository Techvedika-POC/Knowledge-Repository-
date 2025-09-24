using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace KnowLedger_Synaptix.Dtos
{
    public class KnowledgeItemUploadDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? DomainId { get; set; }
        public Guid? CategoryId { get; set; }
        public string Language { get; set; } = string.Empty;
        public bool IsEventRelated { get; set; }
        public Guid? EventId { get; set; }
        public bool IsDraft { get; set; }

        // JSON strings from frontend
        public string Frameworks { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;

        // Add attachments property
        public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
    }

    public class AttachmentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public long FileSize { get; set; }
    }

    public class KnowledgeItemResponseDto
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? DomainId { get; set; }
        public Guid? CategoryId { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Framework { get; set; } = string.Empty;
        public string Metadata { get; set; } = string.Empty;
        public bool IsEventRelated { get; set; }
        public Guid? EventId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
