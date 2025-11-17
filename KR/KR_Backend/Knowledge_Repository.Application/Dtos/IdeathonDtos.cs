using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos
{
    public class AssignMentorDto
    {
        public Guid EventId { get; set; }
        public Guid TeamId { get; set; }
        public List<Guid> MentorIds { get; set; } = new();
    }

    public class CreateJuryDto
    {
        public Guid EventId { get; set; }
        public List<Guid> UserIds { get; set; } = new();
    }

    public class SchedulePresentationDto
    {
        public Guid EventId { get; set; }
        public Guid TeamId { get; set; }
        public DateTime PresentationDate { get; set; }
    }

    public class IdeathonTeamRegistrationDto
    {
        public Guid EventId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public List<Guid> MemberIds { get; set; } = new();
    }
}
