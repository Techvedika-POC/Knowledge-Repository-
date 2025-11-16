using Knowledge_Repository.Application.Dtos.EventInsight;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class EventTeamInsightService : IEventTeamInsightService
    {
        private readonly IEventTeamInsightRepository _insightRepo;

        public EventTeamInsightService(IEventTeamInsightRepository insightRepo)
        {
            _insightRepo = insightRepo;
        }

        public async Task<UserEventInsightDto> GetUserEventInsightAsync(Guid userId, Guid eventId)
        {
            var team = await _insightRepo.GetUserTeamForEventAsync(userId, eventId);
            if (team == null)
                throw new KeyNotFoundException("User is not part of any team for this event.");

            var submissions = await _insightRepo.GetTeamSubmissionsAsync(team.TeamId);
            var feedbacks = await _insightRepo.GetTeamFeedbacksAsync(team.TeamId);
            var avgRating = await _insightRepo.GetTeamAverageRatingAsync(team.TeamId);

            return new UserEventInsightDto
            {
                EventId = eventId,
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                AverageRating = avgRating,
                Submissions = submissions.Select(s => new SubmissionDto
                {
                    ItemId = s.ItemId ?? Guid.Empty,                 // Non-nullable safe
                    Title = s.Item?.Title ?? "(Untitled)",
                    Description = s.Item?.Description ?? "",
                    SubmittedBy = s.Item?.Owner?.Name ?? "Unknown",
                    CreatedBy = s.Item?.CreatedBy,                  // Nullable assignment
                    Tags = s.Item?.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                    Attachments = s.Item?.Attachments?.Select(a => new FileAttachmentDto
                    {
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        MimeType = a.MimeType,
                        FileSize = a.FileSize ?? 0
                    }).ToList() ?? new List<FileAttachmentDto>(),
                    CreatedOn = s.CreatedOn
                }).ToList(),
                Feedbacks = feedbacks.Select(f => new FeedbackDto
                {
                    FeedbackId = f.FeedbackId,
                    MentorName = f.Mentor?.User?.Name ?? "Unknown Mentor",
                    FeedbackText = f.FeedbackText,
                    ProgressRating = f.ProgressRating ?? 0,
                    CreatedOn = f.CreatedOn,
                    Replies = f.TeamFeedbackReplies?.Select(r => new FeedbackReplyDto
                    {
                        ReplyId = r.ReplyId,
                        UserName = r.User?.Name ?? "Unknown",
                        ReplyText = r.ReplyText,
                        CreatedOn = r.CreatedOn
                    }).ToList() ?? new List<FeedbackReplyDto>()
                }).ToList()
            };
        }

        public async Task<List<UserEventInsightDto>> GetEventInsightsAsync(Guid eventId)
        {
            var teams = await _insightRepo.GetTeamsWithDetailsByEventAsync(eventId);

            return teams.Select(team => new UserEventInsightDto
            {
                EventId = eventId,
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                AverageRating = team.TeamFeedbacks.Any(f => f.ProgressRating.HasValue)
                    ? team.TeamFeedbacks.Average(f => f.ProgressRating.Value)
                    : 0,
                Submissions = team.EventKnowledgeItems.Select(s => new SubmissionDto
                {
                    ItemId = s.ItemId ?? Guid.Empty,                 // Non-nullable safe
                    Title = s.Item?.Title ?? "(Untitled)",
                    Description = s.Item?.Description ?? "",
                    SubmittedBy = s.Item?.Owner?.Name ?? "Unknown",
                    CreatedBy = s.Item?.CreatedBy,                  // Nullable assignment
                    Tags = s.Item?.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                    Attachments = s.Item?.Attachments?.Select(a => new FileAttachmentDto
                    {
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        MimeType = a.MimeType,
                        FileSize = a.FileSize ?? 0
                    }).ToList() ?? new List<FileAttachmentDto>(),
                    CreatedOn = s.CreatedOn
                }).ToList(),
                Feedbacks = team.TeamFeedbacks.Select(f => new FeedbackDto
                {
                    FeedbackId = f.FeedbackId,
                    MentorName = f.Mentor?.User?.Name ?? "Unknown Mentor",
                    FeedbackText = f.FeedbackText,
                    ProgressRating = f.ProgressRating ?? 0,
                    CreatedOn = f.CreatedOn,
                    Replies = f.TeamFeedbackReplies?.Select(r => new FeedbackReplyDto
                    {
                        ReplyId = r.ReplyId,
                        UserName = r.User?.Name ?? "Unknown",
                        ReplyText = r.ReplyText,
                        CreatedOn = r.CreatedOn
                    }).ToList() ?? new List<FeedbackReplyDto>()
                }).ToList()
            }).ToList();
        }

        public async Task<bool> SubmitTeamReplyAsync(Guid feedbackId, Guid userId, string replyText)
        {
            if (string.IsNullOrWhiteSpace(replyText))
                throw new ArgumentException("Reply text cannot be empty.");

            var reply = new TeamFeedbackReply
            {
                ReplyId = Guid.NewGuid(),
                FeedbackId = feedbackId,
                TeamId = await _insightRepo.GetTeamIdByFeedbackAsync(feedbackId), // ✅ no '??' needed
                UserId = userId,
                ReplyText = replyText,
                CreatedOn = DateTime.UtcNow
            };

            await _insightRepo.AddFeedbackReplyAsync(reply);
            return true;
        }

        public async Task<bool> SubmitTeamSubmissionAsync(Guid teamId, Guid eventId, string title, string description, Guid createdBy)
        {
            var newSubmission = new EventKnowledgeItem
            {
                EventItemId = Guid.NewGuid(),
                EventId = eventId,
                TeamId = teamId,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy,
                Item = new KnowledgeItem
                {
                    ItemId = Guid.NewGuid(),
                    Title = title,
                    Description = description,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = createdBy
                }
            };

            await _insightRepo.AddTeamSubmissionAsync(newSubmission);
            return true;
        }
    }
}
