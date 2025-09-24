using System;

namespace KnowLedger_Synaptix.Dtos
{
    public class EventDto
    {
        public Guid EventId { get; set; }
        public string Title { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
