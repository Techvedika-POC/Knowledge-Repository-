using System;

namespace Knowledge_Repository.Application.Dtos
{
    public class AIContentOutputDto
    {
        public Guid OutputId { get; set; }
        public Guid RequestId { get; set; }
        public string OutputText { get; set; }
        public string StructuredJson { get; set; }
        public int? TokensUsed { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
