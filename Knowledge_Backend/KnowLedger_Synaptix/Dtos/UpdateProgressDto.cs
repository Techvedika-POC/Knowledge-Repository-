using System;

namespace KnowLedger_Synaptix.Dtos
{
    public class UpdateProgressDto
    {
        public Guid UserId { get; set; }
        public Guid TopicId { get; set; }
        public Guid ModuleId { get; set; }
        public string TestStatus { get; set; } = "Not Started"; // Passed / Failed
    }
}
