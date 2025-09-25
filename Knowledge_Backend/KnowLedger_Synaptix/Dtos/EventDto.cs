using System;

namespace KnowLedger_Synaptix.Dtos
{
    public class EventDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        //public string? EventType { get; set; }   // <-- add this
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? OwnerId { get; set; }
    }
}
