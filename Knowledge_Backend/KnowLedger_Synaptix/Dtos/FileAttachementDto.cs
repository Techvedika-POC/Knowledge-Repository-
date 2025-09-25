using System;
using Microsoft.AspNetCore.Http;

namespace KnowLedger_Synaptix.Dtos
{
    public class FileAttachmentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public long FileSize { get; set; }
    }
}
