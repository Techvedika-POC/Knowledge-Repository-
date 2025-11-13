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
            _context = context;
        }
        public async Task<IEnumerable<Team>> GetAssignedTeamsAsync(Guid mentorId)
        {
            return await _context.Mentors
                .Include(m => m.AssignedTeam)
                    .ThenInclude(t => t.TeamMembers)
                        .ThenInclude(tm => tm.User)
                .Include(m => m.AssignedTeam.Event)
                .Where(m => m.UserId == mentorId)
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
                .Include(t => t.Presentations)
                .FirstOrDefaultAsync(t => t.TeamId == teamId);
        }


        public async Task<IEnumerable<TeamFeedback>> GetTeamFeedbacksAsync(Guid teamId)
        {
            return await _context.Set<TeamFeedback>()
                .Include(f => f.Mentor)
                    .ThenInclude(m => m.User)
                .Where(f => f.TeamId == teamId)
                .OrderByDescending(f => f.CreatedOn)
                .ToListAsync();
        }

        public async Task<IEnumerable<TeamFeedback>> GetFeedbacksByMentorAsync(Guid mentorId)
        {
            return await _context.Set<TeamFeedback>()
                .Include(f => f.Team)
                .Where(f => f.MentorId == mentorId)
                .OrderByDescending(f => f.CreatedOn)
                .ToListAsync();
        }

        public async Task<TeamFeedback> GetFeedbackByIdAsync(Guid feedbackId)
        {
            return await _context.Set<TeamFeedback>()
                .Include(f => f.Team)
                .Include(f => f.Mentor)
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
        }

        public async Task AddFeedbackAsync(TeamFeedback feedback)
        {
            feedback.CreatedOn = DateTime.UtcNow;
            await _context.Set<TeamFeedback>().AddAsync(feedback);
            await _context.SaveChangesAsync();
        }
        public async Task<Mentor> GetMentorByUserIdAsync(Guid userId)
        {
            return await _context.Mentors
                .FirstOrDefaultAsync(m => m.UserId == userId);
        }

        public async Task UpdateFeedbackAsync(TeamFeedback feedback)
        {
            var existing = await _context.Set<TeamFeedback>().FindAsync(feedback.FeedbackId);
            if (existing != null)
            {
                existing.FeedbackText = feedback.FeedbackText;
                existing.ProgressRating = feedback.ProgressRating;
                existing.UpdatedOn = DateTime.UtcNow;

                _context.Set<TeamFeedback>().Update(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteFeedbackAsync(Guid feedbackId)
        {
            var feedback = await _context.Set<TeamFeedback>().FindAsync(feedbackId);
            if (feedback != null)
            {
                _context.Set<TeamFeedback>().Remove(feedback);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<double> GetAverageTeamRatingAsync(Guid teamId)
        {
            var ratings = await _context.Set<TeamFeedback>()
                .Where(f => f.TeamId == teamId && f.ProgressRating.HasValue)
                .Select(f => f.ProgressRating.Value)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0.0;
        }

        public async Task<IEnumerable<(Team team, double averageRating)>> GetTeamsWithProgressAsync(Guid mentorId)
        {
            var teams = await GetAssignedTeamsAsync(mentorId);
            var result = new List<(Team, double)>();

            foreach (var team in teams)
            {
                double avg = await GetAverageTeamRatingAsync(team.TeamId);
                result.Add((team, avg));
            }

            return result;
        }
    }
}
