using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class VersionWithAttachmentsDto
    {
        public Guid VersionId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ChangesSummary { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}
