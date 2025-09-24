using System;
using System.Collections.Generic;

namespace KnowLedger_Synaptix.DTOs
{
    public class KnowledgeItemSearchResultDto
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DomainName { get; set; }
        public string CategoryName { get; set; }
        public List<string> Tags { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class KnowledgeItemSearchRequest
    {
        public string Query { get; set; }
        public Guid? DomainId { get; set; }
        public Guid? CategoryId { get; set; }
        public string ContentType { get; set; }   // Filter on Framework / Language
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
