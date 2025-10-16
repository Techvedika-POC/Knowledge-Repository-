namespace KnowLedger_Synaptix.Dtos
{
    public class ApproverActionDto
    {
        public Guid ItemId { get; set; }   
        public string Action { get; set; } // "Approve" or "Reject"
    }
}
