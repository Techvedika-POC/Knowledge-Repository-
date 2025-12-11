using KnowLedger_Synaptix.Dtos;
namespace Knowledge_Repository.Application.Dtos
{
    public class KnowledgeItemDetailsDto
    {
        public Guid ItemId { get; set; }                
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public Guid? DomainId { get; set; }
        public Guid? CategoryId { get; set; }            
        public bool IsEventItem { get; set; } = false;   
        public Guid? EventId { get; set; }               
        public string ContributorName { get; set; } = "";
        public int EngagementScore { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<AttachmentDto> Attachments { get; set; } = new();
        public string Language { get; set; } = "";
        public string Framework { get; set; } = "";  
        public string Metadata { get; set; } = "";
        public string Visibility { get; set; } = "Private";
        public string CategoryName { get; set; } = "";
        public string DomainName { get; set; } = "";
        public string OwnerName { get; set; } = "";
    }
}