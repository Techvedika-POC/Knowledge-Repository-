using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class EventRegistrationService : IEventRegistrationService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EventRegistrationService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<Team> RegisterTeamForEventAsync(EventRegistrationDto dto, Guid userId)
        {
            // Validate the Event
            var eventEntity = await _context.Events.FindAsync(dto.EventId)
                ?? throw new Exception("Invalid event selected.");

            if (eventEntity.RegistrationCloseDate.HasValue
        && DateTime.UtcNow.Date > eventEntity.RegistrationCloseDate.Value.ToDateTime(new TimeOnly(0, 0)))
            {
                throw new Exception("Registration period for this event has ended.");
            }


            // Check if the user already registered in a team for this event
            bool alreadyInTeam = await _context.TeamMembers
                .Include(tm => tm.Team)
                .AnyAsync(tm => tm.UserId == userId && tm.Team.EventId == dto.EventId);

            if (alreadyInTeam)
                throw new Exception("You are already part of a team for this event.");

            // Check if team name is already taken for this event
            bool duplicateTeam = await _context.Teams
                .AnyAsync(t => t.EventId == dto.EventId && t.TeamName.ToLower() == dto.TeamName.ToLower());

            if (duplicateTeam)
                throw new Exception("A team with this name already exists for this event.");

            // Create new Team
            var team = new Team
            {
                TeamId = Guid.NewGuid(),
                EventId = dto.EventId,
                TeamName = dto.TeamName,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Collect all member emails
            var memberEmails = dto.TeamMemberEmails?
                .SelectMany(e => e.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(e => e.Trim().ToLower())
                .Distinct()
                .ToList() ?? new List<string>();

            // Ensure leader is part of team
            var leaderUser = await _context.Users.FindAsync(userId);
            if (leaderUser == null)
                throw new Exception("Invalid user.");

            var users = await _context.Users
                .Where(u => memberEmails.Contains(u.Email.ToLower()))
                .ToListAsync();

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

            _context.TeamMembers.AddRange(teamMembers);
            await _context.SaveChangesAsync();

            return team;
        }

        public async Task<bool> IsUserAlreadyRegisteredAsync(Guid userId, Guid eventId)
        {
            return await _context.TeamMembers
                .Include(tm => tm.Team)
                .AnyAsync(tm => tm.UserId == userId && tm.Team.EventId == eventId);
        }
    }
}
