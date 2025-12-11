using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos.CommunicationBetweenMentorAndTeam
{
    public class FeedbackReplyDto
    {
        public Guid ReplyId { get; set; }
        public Guid FeedbackId { get; set; }
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }
        public string ReplyText { get; set; } = string.Empty;
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? EditedOn { get; set; }
        public bool IsDeleted { get; set; }
        public string? UserName { get; set; }
    }
}
