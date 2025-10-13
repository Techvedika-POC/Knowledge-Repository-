namespace KnowLedger_Synaptix.Dtos
{
    public class TopicHighlightDto
    {
        public string TopicName { get; set; }
        public int RecentItemCount { get; set; }
        public string[] ExampleContributors { get; set; }
        public int EngagementScore { get; set; } // total engagement for ranking
    }
}
