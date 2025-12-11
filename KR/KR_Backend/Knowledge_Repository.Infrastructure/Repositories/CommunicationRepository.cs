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
    public class CommunicationRepository : ICommunicationRepository
    {
        private readonly Knowledge_Repository_dbContext _context;
        public CommunicationRepository(Knowledge_Repository_dbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<IEnumerable<TeamChatMessage>> GetTeamChatMessagesAsync(Guid teamId)
        {
            return await _context.TeamChatMessages
                .Where(m => m.TeamId == teamId && !m.IsDeleted)
                .OrderBy(m => m.CreatedOn)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<TeamChatMessage> AddTeamChatMessageAsync(TeamChatMessage msg)
        {
            _context.TeamChatMessages.Add(msg);
            await _context.SaveChangesAsync();
            return msg;
        }

        public async Task<TeamChatMessage?> GetChatMessageByIdAsync(Guid messageId)
        {
            return await _context.TeamChatMessages.FindAsync(messageId);
        }

        public async Task<bool> SoftDeleteChatMessageAsync(Guid messageId)
        {
            var m = await _context.TeamChatMessages.FindAsync(messageId);
            if (m == null) return false;
            m.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<TeamFeedback>> GetTeamFeedbacksAsync(Guid teamId)
        {
            return await _context.TeamFeedbacks
                .Where(f => f.TeamId == teamId)
                .OrderBy(f => f.CreatedOn)
                .Include(f => f.Mentor)
                    .ThenInclude(m => m.User)
                .Include(f => f.TeamFeedbackReplies)
                    .ThenInclude(r => r.User)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TeamFeedback> AddTeamFeedbackAsync(TeamFeedback feedback)
        {
            _context.TeamFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<TeamFeedback?> GetTeamFeedbackByIdAsync(Guid feedbackId)
        {
            return await _context.TeamFeedbacks
                .Include(f => f.TeamFeedbackReplies)
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
        }
        public async Task<TeamFeedbackReply> AddTeamFeedbackReplyAsync(TeamFeedbackReply reply)
        {
            _context.TeamFeedbackReplies.Add(reply);
            var fb = await _context.TeamFeedbacks.FindAsync(reply.FeedbackId);
            if (fb != null) fb.LastReplyOn = reply.CreatedOn;
            await _context.SaveChangesAsync();
            return reply;
        }

        public async Task<IEnumerable<TeamFeedbackReply>> GetFeedbackRepliesAsync(Guid feedbackId)
        {
            return await _context.TeamFeedbackReplies
                .Where(r => r.FeedbackId == feedbackId && !r.IsDeleted)
                .OrderBy(r => r.CreatedOn)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
