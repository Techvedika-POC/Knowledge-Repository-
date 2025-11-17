using Knowledge_Repository.Application.Dtos.EventInsight;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IEventTeamInsightService
    {
        Task<UserEventInsightDto> GetUserEventInsightAsync(Guid userId, Guid eventId);
        Task<List<UserEventInsightDto>> GetEventInsightsAsync(Guid eventId);

        // 🆕 New actions
        Task<bool> SubmitTeamReplyAsync(Guid feedbackId, Guid userId, string replyText);
        Task<bool> SubmitTeamSubmissionAsync(Guid teamId, Guid eventId, string title, string description, Guid createdBy);
    }
}
