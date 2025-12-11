using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos.CommunicationBetweenMentorAndTeam
{
    public class ChatMessagesDto
    {
        public Guid MessageId { get; set; }
        public Guid TeamId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? EditedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
