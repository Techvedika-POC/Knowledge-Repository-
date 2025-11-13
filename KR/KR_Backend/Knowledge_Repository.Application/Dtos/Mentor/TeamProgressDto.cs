using System;

namespace Knowledge_Repository.Application.Dtos.Mentor
{
    public class TeamProgressDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
        public double? AverageRating { get; set; }
        public int FeedbackCount { get; set; }
    }
}
