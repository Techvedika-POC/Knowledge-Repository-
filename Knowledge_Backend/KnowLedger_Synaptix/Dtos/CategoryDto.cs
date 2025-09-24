using System;

namespace KnowLedger_Synaptix.Dtos
{
    public class CategoryDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public Guid? DomainId { get; set; }
    }
}
