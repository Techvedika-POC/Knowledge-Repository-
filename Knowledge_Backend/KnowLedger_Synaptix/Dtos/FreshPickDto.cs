namespace KnowLedger_Synaptix.Dtos
{
    public class FreshPickDto
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DomainName { get; set; }
        public string CategoryName { get; set; }
        public string OwnerName { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
