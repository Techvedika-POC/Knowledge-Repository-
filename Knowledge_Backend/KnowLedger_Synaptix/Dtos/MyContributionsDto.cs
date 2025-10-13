namespace KnowLedger_Synaptix.Dtos
{
    public class MyContributionsDto
    {
        public Guid ItemId { get; set; }      // needed for preview/details fetch
        public string Title { get; set; }
        public string Category { get; set; }
        public string Domain { get; set; }
        public string Description { get; set; } // short description
        public string Status { get; set; }
        public DateTime Date { get; set; }
    }
}
