using Knowledge_Repository.Domain.Entities;
using System;
namespace Knowledge_Repository.Application.Dtos
{
    public class EventDto
    {
        public Guid EventId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string EventType { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public Guid? OwnerId { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public Guid? CreatedBy { get; set; }

        public Guid? UpdatedBy { get; set; }

        public DateOnly? RegistrationCloseDate { get; set; }

        public DateOnly? MentorCheckpointStart { get; set; }

        public DateOnly? MentorCheckpointEnd { get; set; }

        public DateOnly? FinalSubmissionDeadline { get; set; }

        public DateOnly? IdeaPresentationStart { get; set; }

        public DateOnly? IdeaPresentationEnd { get; set; }

        public DateOnly? WinnersAnnouncementDate { get; set; }

        public string ContactEmail { get; set; }

        public string Notes { get; set; }

    }
}
