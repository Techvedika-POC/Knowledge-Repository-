public class UserLeaderboardDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Department { get; set; }
    public string ProfileImageUrl { get; set; }
    public int TotalLikesReceived { get; set; }
}
