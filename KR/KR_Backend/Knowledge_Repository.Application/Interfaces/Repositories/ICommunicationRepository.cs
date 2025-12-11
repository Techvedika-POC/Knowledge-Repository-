using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface ICommunicationRepository
    {
        // Chat messages
        Task<IEnumerable<TeamChatMessage>> GetTeamChatMessagesAsync(Guid teamId);
        Task<TeamChatMessage> AddTeamChatMessageAsync(TeamChatMessage msg);
        Task<TeamChatMessage?> GetChatMessageByIdAsync(Guid messageId);
        Task<bool> SoftDeleteChatMessageAsync(Guid messageId);

        // Feedbacks
        Task<IEnumerable<TeamFeedback>> GetTeamFeedbacksAsync(Guid teamId);
        Task<TeamFeedback> AddTeamFeedbackAsync(TeamFeedback feedback);
        Task<TeamFeedback?> GetTeamFeedbackByIdAsync(Guid feedbackId);

        // Replies
        Task<TeamFeedbackReply> AddTeamFeedbackReplyAsync(TeamFeedbackReply reply);
        Task<IEnumerable<TeamFeedbackReply>> GetFeedbackRepliesAsync(Guid feedbackId);
    }
}
