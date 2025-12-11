// File: Knowledge_Repository.Infrastructure.Repositories/MentorRepository.cs
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
    public class MentorRepository : IMentorRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public MentorRepository(Knowledge_Repository_dbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Team>> GetAssignedTeamsAsync(Guid mentorId)
        {
            return await _context.Mentors
                .Where(m => m.UserId == mentorId)
                .Include(m => m.AssignedTeam)
                    .ThenInclude(t => t.TeamMembers)
                        .ThenInclude(tm => tm.User)
                .Include(m => m.AssignedTeam.Event)
                .Select(m => m.AssignedTeam)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Team> GetTeamDetailsAsync(Guid teamId)
        {
            return await _context.Teams
                .Include(t => t.Event)

                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)

                .Include(t => t.Mentors)
                    .ThenInclude(m => m.User)

                .Include(t => t.TeamFeedbacks)
                    .ThenInclude(f => f.Mentor)
                        .ThenInclude(m => m.User)

                .Include(t => t.TeamFeedbackReplies)
                    .ThenInclude(r => r.User)

                .Include(t => t.EventKnowledgeItems)
                    .ThenInclude(eki => eki.Item)
                        .ThenInclude(k => k.KnowledgeTags)

                .Include(t => t.EventKnowledgeItems)
                    .ThenInclude(eki => eki.Item)
                        .ThenInclude(k => k.Engagements)

                .Include(t => t.EventKnowledgeItems)
                    .ThenInclude(eki => eki.Item)
                        .ThenInclude(k => k.Owner)

                .Include(t => t.EventKnowledgeItems)
                    .ThenInclude(eki => eki.Item)
                        .ThenInclude(k => k.Domain)

                .Include(t => t.EventKnowledgeItems)
                    .ThenInclude(eki => eki.Item)
                        .ThenInclude(k => k.Category)

                .FirstOrDefaultAsync(t => t.TeamId == teamId);
        }
        public async Task<string> GetMentorNameAsync(Guid mentorId)
        {
            if (mentorId == Guid.Empty) return null;

            return await _context.Mentors
                .Where(m => m.MentorId == mentorId)
                .Select(m => m.User.Name)   
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetUserNameAsync(Guid userId)
        {
            if (userId == Guid.Empty) return null;

            return await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<Mentor?> GetByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty) return null;
            return await _context.Mentors
                .FirstOrDefaultAsync(m => m.UserId == userId);
        }

        public async Task<Mentor?> GetByUserAndEventAsync(Guid userId, Guid eventId)
        {
            if (userId == Guid.Empty || eventId == Guid.Empty) return null;
            return await _context.Mentors
                .FirstOrDefaultAsync(m => m.UserId == userId && m.EventId == eventId);
        }
        public async Task<Mentor?> GetByIdAsync(Guid mentorId)
        {
            if (mentorId == Guid.Empty) return null;
            return await _context.Mentors.FirstOrDefaultAsync(m => m.MentorId == mentorId);
        }

        public async Task<Mentor?> GetByUserAndTeamAsync(Guid userId, Guid teamId)
        {
            if (userId == Guid.Empty || teamId == Guid.Empty) return null;

            
            var byAssigned = await _context.Mentors.FirstOrDefaultAsync(m => m.UserId == userId && m.AssignedTeamId == teamId);
            if (byAssigned != null) return byAssigned;
     
            return null;
        }
        public async Task<IEnumerable<Mentor>> GetByUserAsync(Guid userId)
        {
            if (userId == Guid.Empty) return Enumerable.Empty<Mentor>();
            return await _context.Mentors.Where(m => m.UserId == userId).ToListAsync();
        }
    }
}
