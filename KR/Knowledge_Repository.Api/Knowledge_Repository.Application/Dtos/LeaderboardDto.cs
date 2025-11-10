namespace Knowledge_Repository.Application.Dtos
{
    public class LeaderboardDto
    {
        public Guid ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public string ItemDescription { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int LikesCount { get; set; }
    }
}
