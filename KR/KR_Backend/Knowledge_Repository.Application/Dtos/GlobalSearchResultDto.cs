namespace Knowledge_Repository.Application.Dtos
{
    public class GlobalSearchResultDto
    {
        public string Type { get; set; }     
        public Guid Id { get; set; }          
        public string Name { get; set; }     
        public string Snippet { get; set; }  
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } 
        public string EventType { get; set; } = string.Empty;
    }
}
