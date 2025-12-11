using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class EventWithTimelineDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateOnly? RegistrationCloseDate { get; set; }
        public DateOnly? MentorCheckpointStart { get; set; }
        public DateOnly? MentorCheckpointEnd { get; set; }
        public DateOnly? FinalSubmissionDeadline { get; set; }
        public DateOnly? IdeaPresentationStart { get; set; }
        public DateOnly? IdeaPresentationEnd { get; set; }
        public DateOnly? WinnersAnnouncementDate { get; set; }

        public string? ContactEmail { get; set; }
        public string? Notes { get; set; }
        public string? EventType { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
