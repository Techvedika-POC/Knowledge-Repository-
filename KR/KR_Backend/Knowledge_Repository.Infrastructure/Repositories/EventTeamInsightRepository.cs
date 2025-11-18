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
    public class EventTeamInsightRepository : IEventTeamInsightRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EventTeamInsightRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<Team?> GetUserTeamForEventAsync(Guid userId, Guid eventId)
        {
            return await _context.TeamMembers
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId && tm.Team.EventId == eventId)
                .Select(tm => tm.Team)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<EventKnowledgeItem>> GetTeamSubmissionsAsync(Guid teamId)
        {
            return await _context.EventKnowledgeItems
                .Include(e => e.Item)
                    .ThenInclude(i => i.Owner) 
                .Include(e => e.Item)
                    .ThenInclude(i => i.KnowledgeTags) 
                .Include(e => e.Item)
                    .ThenInclude(i => i.Attachments)
                .Include(e => e.Event)
                .Where(e => e.TeamId == teamId)
                .OrderByDescending(e => e.CreatedOn)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<TeamFeedback>> GetTeamFeedbacksAsync(Guid teamId)
        {
            return await _context.TeamFeedbacks
                .Include(f => f.Mentor)
                    .ThenInclude(m => m.User)
                .Include(f => f.TeamFeedbackReplies)
                    .ThenInclude(r => r.User)
                .Where(f => f.TeamId == teamId)
                .OrderByDescending(f => f.CreatedOn)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<double> GetTeamAverageRatingAsync(Guid teamId)
        {
            var ratings = await _context.TeamFeedbacks
                .Where(f => f.TeamId == teamId && f.ProgressRating.HasValue)
                .Select(f => f.ProgressRating.Value)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0.0;
        }

        public async Task<IEnumerable<Team>> GetTeamsWithDetailsByEventAsync(Guid eventId)
        {
            return await _context.Teams
                .Include(t => t.TeamMembers)
                .Include(t => t.EventKnowledgeItems)
                    .ThenInclude(eki => eki.Item)
                        .ThenInclude(i => i.Owner)
                .Include(t => t.EventKnowledgeItems)
                    .ThenInclude(eki => eki.Item.KnowledgeTags)
                .Include(t => t.EventKnowledgeItems)
                    .ThenInclude(eki => eki.Item.Attachments)
                .Include(t => t.TeamFeedbacks)
                    .ThenInclude(tf => tf.Mentor)
                        .ThenInclude(m => m.User)
                .Include(t => t.TeamFeedbacks)
                    .ThenInclude(tf => tf.TeamFeedbackReplies)
                        .ThenInclude(r => r.User)
                .Where(t => t.EventId == eventId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Guid> GetTeamIdByFeedbackAsync(Guid feedbackId)
        {
            var teamId = await _context.TeamFeedbacks
                .Where(f => f.FeedbackId == feedbackId)
                .Select(f => f.TeamId)
                .FirstOrDefaultAsync();

            if (teamId == Guid.Empty)
                throw new KeyNotFoundException("No team found for the provided feedback ID.");

            return teamId;
        }

        public async Task<TeamFeedbackReply> AddFeedbackReplyAsync(TeamFeedbackReply reply)
        {
            _context.TeamFeedbackReplies.Add(reply);
            await _context.SaveChangesAsync();
            return reply;
        }

        public async Task<EventKnowledgeItem> AddTeamSubmissionAsync(EventKnowledgeItem submission)
        {
            _context.EventKnowledgeItems.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
