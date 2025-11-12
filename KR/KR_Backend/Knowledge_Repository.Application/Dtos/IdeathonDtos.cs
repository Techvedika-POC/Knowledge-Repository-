using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos
{
    /// <summary>
    /// DTO for assigning one or more mentors to a specific team.
    /// </summary>
    public class AssignMentorDto
    {
        /// <summary>
        /// List of user IDs representing mentors to assign to the team.
        /// </summary>
        public List<Guid> MentorIds { get; set; } = new List<Guid>();
    }

    /// <summary>
    /// DTO for creating a jury for an ideathon event.
    /// </summary>
    public class CreateJuryDto
    {
        /// <summary>
        /// List of user IDs who will be jury members for the event.
        /// </summary>
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }

    /// <summary>
    /// DTO for scheduling a presentation for a team.
    /// </summary>
    public class SchedulePresentationDto
    {
        /// <summary>
        /// The date and time when the team's presentation is scheduled.
        /// </summary>
        public DateTime PresentationDate { get; set; }
    }

    /// <summary>
    /// DTO for registering a team for an ideathon event.
    /// </summary>
    public class IdeathonTeamRegistrationDto
    {
        /// <summary>
        /// The name of the team.
        /// </summary>
        public string TeamName { get; set; } = string.Empty;

        /// <summary>
        /// List of user IDs who are members of the team.
        /// </summary>
        public List<Guid> MemberIds { get; set; } = new List<Guid>();
    }
}
