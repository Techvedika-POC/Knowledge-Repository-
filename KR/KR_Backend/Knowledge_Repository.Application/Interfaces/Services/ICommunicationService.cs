using Knowledge_Repository.Application.Dtos.CommunicationBetweenMentorAndTeam;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ICommunicationService
    {
        Task<IEnumerable<ChatMessagesDto>> GetTeamChatMessagesAsync(Guid userId, Guid teamId);
        Task<ChatMessagesDto> PostTeamChatMessageAsync(Guid userId, Guid teamId, string messageText, string? senderName = null);
        Task<bool> SoftDeleteChatMessageAsync(Guid userId, Guid messageId);

        Task<IEnumerable<FeedbackDto>> GetTeamFeedbacksAsync(Guid userId, Guid teamId);
        Task<FeedbackDto> CreateFeedbackAsync(FeedbackCreateRequest req);
        Task<FeedbackReplyDto> CreateFeedbackReplyAsync(FeedbackReplyCreateRequest req);
    }
}
