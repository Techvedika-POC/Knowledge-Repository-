
using System;
namespace Knowledge_Repository.Application.Dtos
{
    public class EventKnowledgeItemDto
    {
        public Guid EventItemId { get; set; }
        public Guid? EventId { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? TeamId { get; set; }
        public string SubmissionStatus { get; set; } = "Submitted";
    }
}
