namespace KnowLedger_Synaptix.Dtos
{
    public class ActivityLogDto
    {
        public Guid? UserId { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? EventId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
        public string Title { get; set; }
        public string Category { get; set; }
        public string Domain { get; set; }
        public string Description { get; set; } 
        public string Status { get; set; }
        public DateTime? Date { get; set; } 
    }
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
