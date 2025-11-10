
namespace KnowLedger_Synaptix.Dtos
{
    public class VLearnTestResultDto
    {

        public Guid UserId { get; set; }
        public Guid TopicId { get; set; }
        public Guid ModuleId { get; set; }
        public string TestStatus { get; set; } // Passed / Failed

    }
}