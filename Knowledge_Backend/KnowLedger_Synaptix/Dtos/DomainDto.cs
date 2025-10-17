using System;

namespace KnowLedger_Synaptix.Dtos
{
    public class DomainDto
    {
        public Guid DomainId { get; set; }
        public string DomainName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    }
}
