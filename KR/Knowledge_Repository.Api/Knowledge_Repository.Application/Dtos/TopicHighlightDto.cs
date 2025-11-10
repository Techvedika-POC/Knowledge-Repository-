namespace Knowledge_Repository.Application.Dtos
{
    public class TopicHighlightDto
    {
        public string TopicName { get; set; }
        public int RecentItemCount { get; set; }
        public string[] ExampleContributors { get; set; }
        public int EngagementScore { get; set; } 
    }
}