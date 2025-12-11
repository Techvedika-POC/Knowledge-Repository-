using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos.CommunicationBetweenMentorAndTeam
{
    public class FeedbackReplyCreateRequest
    {
        public Guid FeedbackId { get; set; }
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }
        public string ReplyText { get; set; } = string.Empty;
    }
}
