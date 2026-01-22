using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class IdeathonRepository : IIdeathonRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public IdeathonRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAvailableMentorsAsync(Guid eventId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.RoleName.ToLower() == "mentor")
                .Select(ur => ur.User)
                .ToListAsync();
        }

        public async Task AssignMentorToTeamAsync(Guid eventId, Guid teamId, Guid mentorUserId)
        {
            bool alreadyAssigned = await _context.Mentors
                .AnyAsync(m => m.EventId == eventId && m.AssignedTeamId == teamId && m.UserId == mentorUserId);
            if (alreadyAssigned) return;

            var mentor = new Mentor
            {
                EventId = eventId,
                AssignedTeamId = teamId,
                UserId = mentorUserId,
                CreatedOn = DateTime.UtcNow
            };

            await _context.Mentors.AddAsync(mentor);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Mentor>> GetMentorsForEventAsync(Guid eventId)
        {
            return await _context.Mentors
                .Include(m => m.User)
                .Include(m => m.AssignedTeam)
                .Where(m => m.EventId == eventId)
                .ToListAsync();
        }

        public async Task RemoveMentorFromTeamAsync(Guid mentorId)
        {
            var mentor = await _context.Mentors
                .FirstOrDefaultAsync(x => x.MentorId == mentorId);

            if (mentor == null)
                throw new ArgumentException("Mentor not found.");

            mentor.AssignedTeamId = null;
            mentor.EventId = null;

            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<User>> GetAvailableJuryMembersAsync(Guid eventId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.RoleName.ToLower() == "jurymember")
                .Select(ur => ur.User)
                .ToListAsync();
        }

        public async Task AssignJuryMembersToEventAsync(Guid eventId, IEnumerable<Guid> juryUserIds)
        {
            var existingJuries = await _context.EventJuries
                .Where(j => j.EventId == eventId)
                .Select(j => j.UserId)
                .ToListAsync();

            var newJuryMembers = juryUserIds
         .Where(id => !existingJuries.Contains(id))
         .Select(id => new EventJury
         {
             EventJuryId = Guid.NewGuid(),
             EventId = eventId,
             UserId = id,
             AssignedOn = DateTime.UtcNow
         });

            await _context.EventJuries.AddRangeAsync(newJuryMembers);
            await _context.SaveChangesAsync();
            ;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetJuryMembersForEventAsync(Guid eventId)
        {
            if (eventId == Guid.Empty) throw new ArgumentException(nameof(eventId));

            var roleNameVariants = new[] { "jurymember", "jury member", "jury" }; 
            var roleUsersQuery = _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => roleNameVariants.Contains(ur.Role.RoleName.ToLower()))
                .Select(ur => ur.User);

            var eventAssignedQuery = _context.EventJuries
                .Include(ej => ej.User)
                .Where(ej => ej.EventId == eventId)
                .Select(ej => ej.User);
            var roleUsers = await roleUsersQuery.ToListAsync();
            var eventAssigned = await eventAssignedQuery.ToListAsync();

            var combined = roleUsers
                .Concat(eventAssigned)
                .GroupBy(u => u.UserId)
                .Select(g => g.First())
                .ToList();

            return combined;
        }


        public async Task RemoveJuryMemberFromEventAsync(Guid eventId, Guid userId)
        {
            var juryMember = await _context.EventJuries
                .FirstOrDefaultAsync(ej => ej.EventId == eventId && ej.UserId == userId);

            if (juryMember != null)
            {
                _context.EventJuries.Remove(juryMember);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Team>> GetTeamsForEventAsync(Guid eventId)
        {
            return await _context.Teams
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .Include(t => t.Mentors)
                    .ThenInclude(m => m.User)
                .Include(t => t.Presentations)
                .Where(t => t.EventId == eventId)
                .ToListAsync();
        }

        public async Task<Team?> GetTeamByIdAsync(Guid teamId)
        {
            return await _context.Teams
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .Include(t => t.Mentors)
                    .ThenInclude(m => m.User)
                .Include(t => t.Presentations)
                .FirstOrDefaultAsync(t => t.TeamId == teamId);
        }

        public async Task SchedulePresentationAsync(Guid eventId, Guid teamId, DateTime presentationDate)
        {
            var existing = await _context.Presentations
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.TeamId == teamId);

            if (existing != null)
            {
                existing.PresentationDate = presentationDate;
                existing.CreatedOn = DateTime.UtcNow;
                _context.Presentations.Update(existing);
            }
            else
            {
                var presentation = new Presentation
                {
                    PresentationId = Guid.NewGuid(),
                    EventId = eventId,
                    TeamId = teamId,
                    PresentationDate = presentationDate,
                    CreatedOn = DateTime.UtcNow
                };
                await _context.Presentations.AddAsync(presentation);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Presentation>> GetPresentationsForEventAsync(Guid eventId)
        {
            return await _context.Presentations
                .Include(p => p.Team)
                .Where(p => p.EventId == eventId)
                .OrderBy(p => p.PresentationDate)
                .ToListAsync();
        }
        public async Task RemovePresentationAsync(Guid presentationId)
        {
            if (presentationId == Guid.Empty) return;

            var pres = await _context.Presentations.FindAsync(presentationId);
            if (pres == null) return;

            _context.Presentations.Remove(pres);
            await _context.SaveChangesAsync();
        }
       

    }
}
