using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{



    public class FeedbackReplyDtos
    {
        public Guid ReplyId { get; set; }
        public Guid FeedbackId { get; set; }
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }
        public string ReplyText { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}
