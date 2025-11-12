using System;
using System.ComponentModel.DataAnnotations;

namespace Knowledge_Repository.Application.Dtos.Mentor
{
    public class AddFeedbackRequestDto
    {
        [Required]
        public Guid MentorId { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [Required]
        public string FeedbackText { get; set; }

        public int? ProgressRating { get; set; }
    }
}
