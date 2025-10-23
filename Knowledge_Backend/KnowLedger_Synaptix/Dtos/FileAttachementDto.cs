using System;

namespace KnowLedger_Synaptix.Dtos
{
    public class FileAttachmentDto
    {
        public Guid FileAttachmentId { get; set; }
        public Guid? LinkedEntityId { get; set; }
        public Guid? TeamId { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public long FileSize { get; set; }
        public string FilePath { get; set; } = string.Empty;

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
