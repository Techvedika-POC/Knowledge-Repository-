namespace KnowLedger_Synaptix.Dtos
{
    public class MyContributionsDto
    {
        public Guid ItemId { get; set; }     
        public string Title { get; set; }
        public string Category { get; set; }
        public string Domain { get; set; }
        public string Description { get; set; } 
        public string Status { get; set; }
        public DateTime Date { get; set; }
    }
}
