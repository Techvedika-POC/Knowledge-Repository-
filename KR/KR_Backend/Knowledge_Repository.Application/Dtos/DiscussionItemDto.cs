using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class DiscussionItemDto
    {
        public Guid ItemId { get; set; }
        public Guid TeamId { get; set; }
        public string Type { get; set; }           // "Chat" | "Feedback" | "FeedbackReply"
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public Guid? ParentFeedbackId { get; set; } // for replies
    }

    

 
}
