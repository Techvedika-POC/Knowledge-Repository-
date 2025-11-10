using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class EventRegistrationService : IEventRegistrationService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IUserRepository _userRepository;

        public EventRegistrationService(
            IEventRepository eventRepository,
            ITeamRepository teamRepository,
            ITeamMemberRepository teamMemberRepository,
            IUserRepository userRepository)
        {
            _eventRepository = eventRepository;
            _teamRepository = teamRepository;
            _teamMemberRepository = teamMemberRepository;
            _userRepository = userRepository;
        }

        public async Task<Team> RegisterTeamForEventAsync(EventRegistrationDto dto, Guid userId)
        {
            // Validate Event
            var eventEntity = await _eventRepository.GetByIdAsync(dto.EventId)
                ?? throw new Exception("Invalid event selected.");

            if (eventEntity.RegistrationCloseDate.HasValue &&
                DateTime.UtcNow.Date > eventEntity.RegistrationCloseDate.Value.ToDateTime(new TimeOnly(0, 0)))
            {
                throw new Exception("Registration period for this event has ended.");
            }

            // Check if user already registered
            bool alreadyInTeam = (await _teamMemberRepository.GetAllAsync(
                tm => tm.UserId == userId && tm.Team.EventId == dto.EventId))
                .Any();

            if (alreadyInTeam)
                throw new Exception("You are already part of a team for this event.");

            //  Ensure team name uniqueness
            bool duplicateTeam = (await _teamRepository.GetAllAsync(
                t => t.EventId == dto.EventId && t.TeamName.ToLower() == dto.TeamName.ToLower()))
                .Any();

            if (duplicateTeam)
                throw new Exception("A team with this name already exists for this event.");

            //  Create new Team
            var team = new Team
            {
                TeamId = Guid.NewGuid(),
                EventId = dto.EventId,
                TeamName = dto.TeamName,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            await _teamRepository.AddAsync(team);

            //  Add team members
            var memberEmails = dto.TeamMemberEmails?
                .SelectMany(e => e.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(e => e.Trim().ToLower())
                .Distinct()
                .ToList() ?? new List<string>();

            var leaderUser = await _userRepository.GetByIdAsync(userId)
                ?? throw new Exception("Invalid user.");

            var users = (await _userRepository.GetAllAsync(u => memberEmails.Contains(u.Email.ToLower())))
                .ToList();

            var teamMembers = new List<TeamMember>
            {
                new TeamMember
                {
                    TeamMemberId = Guid.NewGuid(),
                    TeamId = team.TeamId,
                    UserId = leaderUser.UserId,
                    Role = "Leader",
                    JoinedOn = DateTime.UtcNow
                }
            };

            foreach (var user in users)
            {
                if (user.UserId == leaderUser.UserId) continue;

                teamMembers.Add(new TeamMember
                {
                    TeamMemberId = Guid.NewGuid(),
                    TeamId = team.TeamId,
                    UserId = user.UserId,
                    Role = "Member",
                    JoinedOn = DateTime.UtcNow
                });
            }

            await _teamMemberRepository.AddRangeAsync(teamMembers);
            return team;
        }

        public async Task<bool> IsUserAlreadyRegisteredAsync(Guid userId, Guid eventId)
        {
            return (await _teamMemberRepository.GetAllAsync(
                tm => tm.UserId == userId && tm.Team.EventId == eventId)).Any();
        }
    }
}
