using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos.Mentor
{
    public class TeamDetailsDto
    {
        public Guid TeamId { get; set; }
        public Guid? EventId { get; set; }        
        public string? Description { get; set; }    
        public string TeamName { get; set; }
        public string? ProjectTitle { get; set; }

        public IEnumerable<MemberDto>? Members { get; set; }
        public IEnumerable<FeedbackResponseDto>? Feedbacks { get; set; }
    }
}
