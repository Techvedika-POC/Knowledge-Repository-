using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos.CommunicationBetweenMentorAndTeam
{
    public class FeedbackDto
    {
        public Guid FeedbackId { get; set; }
        public Guid MentorId { get; set; }
        public Guid TeamId { get; set; }
        public Guid EventId { get; set; }
        public string FeedbackText { get; set; } = string.Empty;
        public int? ProgressRating { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
        public DateTimeOffset? LastReplyOn { get; set; }
        public IEnumerable<FeedbackReplyDto> Replies { get; set; } = new List<FeedbackReplyDto>();
    }
}
