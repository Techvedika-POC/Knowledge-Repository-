namespace KnowLedger_Synaptix.Dtos
{
    public class ApproverActionDto
    {
        public Guid ItemId { get; set; }   // The knowledge item ID
        public string Action { get; set; } // "Approve" or "Reject"
    }
}
