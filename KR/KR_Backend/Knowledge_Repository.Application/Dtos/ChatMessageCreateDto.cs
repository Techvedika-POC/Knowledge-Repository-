using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
    public class ChatMessageCreateDto
    {
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
    }
}
