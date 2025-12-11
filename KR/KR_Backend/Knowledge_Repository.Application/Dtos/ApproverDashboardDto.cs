namespace Knowledge_Repository.Application.Dtos
{
    public class ApproverDashboardDto
    {
        public int TotalPending { get; set; }
        public int NormalPending { get; set; }
        public int EventPending { get; set; }
        public List<EventPendingCountDto> EventWiseCounts { get; set; }
    }

    public class EventPendingCountDto
    {
        public Guid EventId { get; set; }
        public string EventTitle { get; set; }
        public int PendingCount { get; set; }
    }
}
