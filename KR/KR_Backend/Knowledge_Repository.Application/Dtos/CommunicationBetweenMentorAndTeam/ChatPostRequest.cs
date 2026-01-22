using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos.CommunicationBetweenMentorAndTeam
{
    public class ChatPostRequest
    {
        public string MessageText { get; set; } = string.Empty;
        public string? SenderName { get; set; } 
    }
}
