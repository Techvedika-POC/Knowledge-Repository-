using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class IdeathonService : IIdeathonService
    {
        private readonly IIdeathonRepository _repo;

        public IdeathonService(IIdeathonRepository repo)
        {
            _repo = repo;
        }

        // -------------------- MENTORS --------------------
        public async Task<IEnumerable<User>> GetAvailableMentorsAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetAvailableMentorsAsync(eventId);
        }

        public async Task AssignMentorToTeamAsync(Guid eventId, Guid teamId, Guid mentorUserId)
        {
            if (eventId == Guid.Empty || teamId == Guid.Empty || mentorUserId == Guid.Empty)
                throw new ArgumentException("Invalid mentor, event, or team ID.");

            await _repo.AssignMentorToTeamAsync(eventId, teamId, mentorUserId);
        }

        public async Task<IEnumerable<Mentor>> GetMentorsForEventAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetMentorsForEventAsync(eventId);
        }

        public async Task RemoveMentorFromTeamAsync(Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                throw new ArgumentException("Invalid mentor ID.", nameof(mentorId));

            await _repo.RemoveMentorFromTeamAsync(mentorId);
        }

        // -------------------- JURY --------------------
        public async Task<IEnumerable<User>> GetAvailableJuryMembersAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetAvailableJuryMembersAsync(eventId);
        }

        public async Task AssignJuryMembersToEventAsync(Guid eventId, IEnumerable<Guid> juryUserIds)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            if (juryUserIds == null)
                throw new ArgumentNullException(nameof(juryUserIds));

            var userList = new List<Guid>(juryUserIds);

            if (userList.Count == 0)
                throw new ArgumentException("At least one jury member must be selected.", nameof(juryUserIds));

            await _repo.AssignJuryMembersToEventAsync(eventId, userList);
        }

        public async Task<IEnumerable<User>> GetJuryMembersForEventAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetJuryMembersForEventAsync(eventId);
        }

        public async Task RemoveJuryMemberFromEventAsync(Guid eventId, Guid userId)
        {
            if (eventId == Guid.Empty || userId == Guid.Empty)
                throw new ArgumentException("Invalid event or jury member ID.");

            await _repo.RemoveJuryMemberFromEventAsync(eventId, userId);
        }

        // -------------------- TEAMS --------------------
        public async Task<IEnumerable<Team>> GetTeamsForEventAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetTeamsForEventAsync(eventId);
        }

        public async Task<Team?> GetTeamByIdAsync(Guid teamId)
        {
            if (teamId == Guid.Empty)
                throw new ArgumentException("Invalid team ID.", nameof(teamId));

            return await _repo.GetTeamByIdAsync(teamId);
        }

        // -------------------- PRESENTATIONS --------------------
        public async Task SchedulePresentationAsync(Guid eventId, Guid teamId, DateTime presentationDate)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            if (teamId == Guid.Empty)
                throw new ArgumentException("Invalid team ID.", nameof(teamId));

            if (presentationDate == default)
                throw new ArgumentException("Invalid presentation date.", nameof(presentationDate));

            await _repo.SchedulePresentationAsync(eventId, teamId, presentationDate);
        }

        public async Task<IEnumerable<Presentation>> GetPresentationsForEventAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(eventId));

            return await _repo.GetPresentationsForEventAsync(eventId);
        }
    }
}
