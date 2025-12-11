using System;

namespace Knowledge_Repository.Application.Dtos
{
    public class AIContentRequestDto
    {
        public Guid RequestId { get; set; }
        public string RequestType { get; set; }
        public string Prompt { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
