using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Domain.Entities
{
    public class TeamChatMessage
    {
        public Guid MessageId { get; set; }
        public Guid TeamId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string MessageText { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? EditedOn { get; set; }   
        public bool IsDeleted { get; set; }
    }
}
