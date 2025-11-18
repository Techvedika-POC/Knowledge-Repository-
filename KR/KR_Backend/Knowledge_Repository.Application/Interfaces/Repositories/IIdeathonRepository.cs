using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IIdeathonRepository
    {
        // Mentor related
        Task<IEnumerable<User>> GetAvailableMentorsAsync(Guid eventId);
        Task AssignMentorToTeamAsync(Guid eventId, Guid teamId, Guid mentorUserId);
        Task<IEnumerable<Mentor>> GetMentorsForEventAsync(Guid eventId);
        Task RemoveMentorFromTeamAsync(Guid mentorId);

        // Jury related
        Task<IEnumerable<User>> GetAvailableJuryMembersAsync(Guid eventId);
        Task AssignJuryMembersToEventAsync(Guid eventId, IEnumerable<Guid> juryUserIds);
        Task<IEnumerable<User>> GetJuryMembersForEventAsync(Guid eventId);
        Task RemoveJuryMemberFromEventAsync(Guid eventId, Guid userId);

        // Team related
        Task<IEnumerable<Team>> GetTeamsForEventAsync(Guid eventId);
        Task<Team?> GetTeamByIdAsync(Guid teamId);

        // Presentation related
        Task SchedulePresentationAsync(Guid eventId, Guid teamId, DateTime presentationDate);
        Task RemovePresentationAsync(Guid presentationId);
        Task<IEnumerable<Presentation>> GetPresentationsForEventAsync(Guid eventId);
    }
}
