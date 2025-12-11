using System;

namespace Knowledge_Repository.Application.Dtos
{
    public class ResourceDto
    {
        public Guid ResourceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ResourceType { get; set; } = "Link"; 
        public Guid TopicId { get; set; }
        public Guid? ModuleId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsAiGenerated { get; set; } = false;
        public string Metadata { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
