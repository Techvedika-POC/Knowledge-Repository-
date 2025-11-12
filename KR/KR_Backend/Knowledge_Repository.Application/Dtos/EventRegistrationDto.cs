
using System;
using System.Collections.Generic;
namespace Knowledge_Repository.Application.Dtos
{
    public class EventRegistrationDto
    {
        public Guid EventId { get; set; }
        public string TeamName { get; set; }
        public Guid CreatedBy { get; set; }  // User ID  of the team lead
        public List<string> TeamMemberEmails { get; set; } = new List<string>();
    }
}
