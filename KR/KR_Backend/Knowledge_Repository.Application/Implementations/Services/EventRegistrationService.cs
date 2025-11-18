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
            
            var eventEntity = await _eventRepository.GetByIdAsync(dto.EventId)
                ?? throw new Exception("Invalid event selected.");

            if (eventEntity.RegistrationCloseDate.HasValue &&
                DateTime.UtcNow.Date > eventEntity.RegistrationCloseDate.Value.ToDateTime(new TimeOnly(0, 0)))
            {
                throw new Exception("Registration period for this event has ended.");
            }
            var memberEmails = dto.TeamMemberEmails?
                .SelectMany(e => e.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(e => e.Trim().ToLowerInvariant())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToList() ?? new List<string>();
            var leaderUser = await _userRepository.GetByIdAsync(userId)
                ?? throw new Exception("Invalid user.");

            var leaderEmailLower = leaderUser.Email.ToLowerInvariant();
            if (!memberEmails.Contains(leaderEmailLower))
                memberEmails.Insert(0, leaderEmailLower);

            // 3) Resolve emails -> users; reject if some emails don't map to actual users
            var users = (await _userRepository.GetAllAsync(u => memberEmails.Contains(u.Email.ToLower())))
                        .ToList();

            var emailToUser = users.ToDictionary(u => u.Email.ToLower(), u => u);
            var unresolved = memberEmails.Where(e => !emailToUser.ContainsKey(e)).ToList();
            if (unresolved.Any())
            {
                throw new Exception($"These emails are not registered users: {string.Join(", ", unresolved)}");
            }

            // Member userIds list (sorted for deterministic signature)
            var memberUserIds = users.Select(u => u.UserId).Distinct().ToList();
            memberUserIds.Sort(); // ensure deterministic order

            // 4) Check: leader (requesting user) cannot already be part of a different team for the SAME event
            bool leaderAlreadyInEvent = (await _teamMemberRepository.GetAllAsync(
                tm => tm.UserId == userId && tm.Team.EventId == dto.EventId)).Any();

            if (leaderAlreadyInEvent)
                throw new Exception("You are already part of a team for this event.");

            // 5) Check: none of the supplied members may already be in another team for the SAME event
            var conflictMembersInSameEvent = (await _teamMemberRepository.GetAllAsync(
                tm => memberUserIds.Contains(tm.UserId) && tm.Team.EventId == dto.EventId))
                .Select(tm => tm.UserId)
                .Distinct()
                .ToList();

            if (conflictMembersInSameEvent.Any())
            {
                var conflictingEmails = users
                    .Where(u => conflictMembersInSameEvent.Contains(u.UserId))
                    .Select(u => u.Email)
                    .ToList();

                throw new Exception($"These member(s) are already registered in a team for this event: {string.Join(", ", conflictingEmails)}");
            }

            // 6) Check: the same team (identical member set) should not already exist for ANY other event
            var relatedTeamMembers = (await _teamMemberRepository.GetAllAsync(
                tm => memberUserIds.Contains(tm.UserId)))
                .ToList();

            var teamsGrouped = relatedTeamMembers
                .GroupBy(tm => tm.TeamId)
                .Select(g => new
                {
                    TeamId = g.Key,
                    MemberIds = g.Select(x => x.UserId).Distinct().OrderBy(id => id).ToList()
                })
                .ToList();

            var sameSizeCandidates = teamsGrouped
                .Where(tg => tg.MemberIds.Count == memberUserIds.Count)
                .ToList();

            var identicalTeamIds = new List<Guid>();
            foreach (var cand in sameSizeCandidates)
            {
                var sortedCand = cand.MemberIds.OrderBy(id => id).ToList();
                bool equal = sortedCand.SequenceEqual(memberUserIds);
                if (equal)
                    identicalTeamIds.Add(cand.TeamId);
            }

            if (identicalTeamIds.Any())
            {
                var identicalTeams = (await _teamRepository.GetAllAsync(t => identicalTeamIds.Contains(t.TeamId))).ToList();

                var conflictsAcrossEvents = identicalTeams
                    .Where(t => t.EventId != dto.EventId) // teams in other events
                    .ToList();

                if (conflictsAcrossEvents.Any())
                {
                    var conflictingTeamInfo = conflictsAcrossEvents
                        .Select(t => $"Team '{t.TeamName}' (EventId: {t.EventId})")
                        .ToList();

                    throw new Exception($"An identical team (same members) is already registered for another event: {string.Join("; ", conflictingTeamInfo)}");
                }

                throw new Exception("An identical team (same members) is already registered for this event.");
            }

            // 7) Check duplicate team name for same event
            bool duplicateTeamName = (await _teamRepository.GetAllAsync(
                t => t.EventId == dto.EventId && t.TeamName.ToLower() == dto.TeamName.ToLower()))
                .Any();

            if (duplicateTeamName)
                throw new Exception("A team with this name already exists for this event.");

            // 8) Create team + members using repository methods (no transaction)
            var team = new Team
            {
                TeamId = Guid.NewGuid(),
                EventId = dto.EventId,
                TeamName = dto.TeamName,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            // Add team
            await _teamRepository.AddAsync(team);

            var teamMembers = new List<TeamMember>();

            // leader
            var leader = emailToUser[leaderEmailLower];
            teamMembers.Add(new TeamMember
            {
                TeamMemberId = Guid.NewGuid(),
                TeamId = team.TeamId,
                UserId = leader.UserId,
                Role = "Leader",
                JoinedOn = DateTime.UtcNow
            });

            // other members
            foreach (var u in users.Where(u => u.UserId != leader.UserId))
            {
                teamMembers.Add(new TeamMember
                {
                    TeamMemberId = Guid.NewGuid(),
                    TeamId = team.TeamId,
                    UserId = u.UserId,
                    Role = "Member",
                    JoinedOn = DateTime.UtcNow
                });
            }

            // Final race-check (best-effort without transaction) — optional but helpful
            var memberIds = teamMembers.Select(tm => tm.UserId).ToList();
            var raceConflicts = (await _teamMemberRepository.GetAllAsync(
                tm => memberIds.Contains(tm.UserId) && tm.Team.EventId == dto.EventId)).ToList();

            if (raceConflicts.Any())
            {
                var conflictIds = raceConflicts.Select(r => r.UserId).Distinct().ToHashSet();
                var conflictEmailsNow = users.Where(u => conflictIds.Contains(u.UserId)).Select(u => u.Email).ToList();
                // If conflicts found, do not add members; you may want to remove the team you added (but without transactions this is best-effort)
                throw new Exception($"Registration failed; these members were just registered in another team for this event: {string.Join(", ", conflictEmailsNow)}");
            }

            // Add members
            await _teamMemberRepository.AddRangeAsync(teamMembers);

            // Return created team (assuming repositories persist)
            return team;
        }

        public async Task<bool> IsUserAlreadyRegisteredAsync(Guid userId, Guid eventId)
        {
            return (await _teamMemberRepository.GetAllAsync(
                tm => tm.UserId == userId && tm.Team.EventId == eventId)).Any();
        }
    }
}
