using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Domain.Entities
{
    public partial class JuryChatMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public Guid SenderJuryId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // new nullable foreign key pointing to another message
        public Guid? ReplyToMessageId { get; set; }

        // Navigation properties
        public virtual Event Event { get; set; }
        public virtual User Sender { get; set; }

        // Self-referencing navigation
        public virtual JuryChatMessage ReplyTo { get; set; }

        // inverse nav: messages that reply to this message
        public virtual ICollection<JuryChatMessage> Replies { get; set; } = new List<JuryChatMessage>();
    }
}
