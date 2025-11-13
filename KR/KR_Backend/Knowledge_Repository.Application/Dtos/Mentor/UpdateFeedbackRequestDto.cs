using System;
using System.ComponentModel.DataAnnotations;

namespace Knowledge_Repository.Application.Dtos.Mentor
{
    public class UpdateFeedbackRequestDto
    {
        [Required]
        public Guid FeedbackId { get; set; }

        [Required]
        public string FeedbackText { get; set; }

        public int? ProgressRating { get; set; }
    }
}
