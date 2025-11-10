namespace Knowledge_Repository.Application.Dtos
{
    public class KnowledgeItemFilterDto
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DomainName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.MinValue;
        public bool IsEventItem { get; set; } = false;   // <-- added
        public List<string> Tags { get; set; } = new List<string>();
    }
}
