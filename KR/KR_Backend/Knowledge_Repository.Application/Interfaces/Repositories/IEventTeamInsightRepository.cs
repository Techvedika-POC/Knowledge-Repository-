using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IEventTeamInsightRepository
    {
        Task<Team?> GetUserTeamForEventAsync(Guid userId, Guid eventId);
        Task<IEnumerable<EventKnowledgeItem>> GetTeamSubmissionsAsync(Guid teamId);
        Task<IEnumerable<TeamFeedback>> GetTeamFeedbacksAsync(Guid teamId);
        Task<double> GetTeamAverageRatingAsync(Guid teamId);
        Task<IEnumerable<Team>> GetTeamsWithDetailsByEventAsync(Guid eventId);

        // 🆕 New methods
        Task<Guid> GetTeamIdByFeedbackAsync(Guid feedbackId);
        Task<TeamFeedbackReply> AddFeedbackReplyAsync(TeamFeedbackReply reply);
        Task<EventKnowledgeItem> AddTeamSubmissionAsync(EventKnowledgeItem submission);
        Task SaveChangesAsync();
    }
}
