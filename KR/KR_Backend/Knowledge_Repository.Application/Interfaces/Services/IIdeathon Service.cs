using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IIdeathonService
    {

        Task<IEnumerable<User>> GetAvailableMentorsAsync(Guid eventId);
        Task AssignMentorToTeamAsync(Guid eventId, Guid teamId, Guid mentorUserId);
        Task<IEnumerable<Mentor>> GetMentorsForEventAsync(Guid eventId);

        Task RemoveMentorFromTeamAsync(Guid mentorId);

        Task<IEnumerable<User>> GetAvailableJuryMembersAsync(Guid eventId);
        Task AssignJuryMembersToEventAsync(Guid eventId, IEnumerable<Guid> juryUserIds);

        Task<IEnumerable<User>> GetJuryMembersForEventAsync(Guid eventId);
        Task RemoveJuryMemberFromEventAsync(Guid eventId, Guid userId);
        Task<IEnumerable<Team>> GetTeamsForEventAsync(Guid eventId);
        Task<Team?> GetTeamByIdAsync(Guid teamId);

        Task SchedulePresentationAsync(Guid eventId, Guid teamId, DateTime presentationDate);
        Task<IEnumerable<Presentation>> GetPresentationsForEventAsync(Guid eventId);
    }
}
