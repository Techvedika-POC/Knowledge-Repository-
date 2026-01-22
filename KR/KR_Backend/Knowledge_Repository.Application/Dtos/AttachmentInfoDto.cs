using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class AttachmentInfoDto
    {
        public Guid AttachmentId { get; set; }
        public string? FileName { get; set; }
        public string? MimeType { get; set; }
        public long? FileSize { get; set; }
        public string? FileUrl { get; set; }        
        public string? FilePath { get; set; }       
        public string? PhysicalPath { get; set; }
        public byte[]? FileData { get; set; }
    }
}
