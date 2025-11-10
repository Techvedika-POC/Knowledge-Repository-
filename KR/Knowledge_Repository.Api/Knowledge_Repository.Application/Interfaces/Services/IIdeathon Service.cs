using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IIdeathonService
    {
        // -------------------- MENTORS --------------------
        /// <summary>Fetches all available mentors for a specific event.</summary>
        Task<IEnumerable<User>> GetAvailableMentorsAsync(Guid eventId);

        /// <summary>Assigns a mentor to a specific team in an event.</summary>
        Task AssignMentorToTeamAsync(Guid eventId, Guid teamId, Guid mentorUserId);

        /// <summary>Gets all mentors assigned to teams within an event.</summary>
        Task<IEnumerable<Mentor>> GetMentorsForEventAsync(Guid eventId);

        /// <summary>Removes a mentor assignment from a team.</summary>
        Task RemoveMentorFromTeamAsync(Guid mentorId);

        // -------------------- JURY --------------------
        /// <summary>Fetches all users with a Jury role who are available for an event.</summary>
        Task<IEnumerable<User>> GetAvailableJuryMembersAsync(Guid eventId);

        /// <summary>Assigns one or more jury members to an event.</summary>
        Task AssignJuryMembersToEventAsync(Guid eventId, IEnumerable<Guid> juryUserIds);

        /// <summary>Gets all jury members assigned to a specific event.</summary>
        Task<IEnumerable<User>> GetJuryMembersForEventAsync(Guid eventId);

        /// <summary>Removes a jury member from an event.</summary>
        Task RemoveJuryMemberFromEventAsync(Guid eventId, Guid userId);

        // -------------------- TEAMS --------------------
        Task<IEnumerable<Team>> GetTeamsForEventAsync(Guid eventId);
        Task<Team?> GetTeamByIdAsync(Guid teamId);

        // -------------------- PRESENTATIONS --------------------
        /// <summary>Schedules or updates a presentation for a team in an event.</summary>
        Task SchedulePresentationAsync(Guid eventId, Guid teamId, DateTime presentationDate);

        /// <summary>Fetches all presentations scheduled for a specific event.</summary>
        Task<IEnumerable<Presentation>> GetPresentationsForEventAsync(Guid eventId);
    }
}
