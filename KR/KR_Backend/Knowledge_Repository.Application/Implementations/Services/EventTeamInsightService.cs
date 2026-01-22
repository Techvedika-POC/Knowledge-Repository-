using Knowledge_Repository.Application.Dtos.EventInsight;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Application.Dtos;
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
        private readonly ITeamMemberRepository _teamMemberRepo;

        public EventTeamInsightService(IEventTeamInsightRepository insightRepo, ITeamMemberRepository teamMemberRepo)
        {
            _insightRepo = insightRepo;
            _teamMemberRepo = teamMemberRepo;
        }
        public async Task<UserEventInsightDto> GetUserEventInsightAsync(Guid userId, Guid eventId)
        {
            var team = await _insightRepo.GetUserTeamForEventAsync(userId, eventId);
            if (team == null)
                throw new KeyNotFoundException("User is not part of any team for this event.");
            var teamMembersDomain = await _teamMemberRepo.GetMembersByTeamIdAsync(team.TeamId);

    
            var submissions = await _insightRepo.GetTeamSubmissionsAsync(team.TeamId);

            var avgRating = await _insightRepo.GetTeamAverageRatingAsync(team.TeamId);

        
            var teamMemberDtos = teamMembersDomain.Select(tm => new TeamMemberDto
            {
                UserId = tm.UserId,
                Name = tm.User?.Name ?? string.Empty,
                Role = tm.Role ?? string.Empty,
                Email = tm.User?.Email ?? string.Empty
            }).ToList();

            var submissionDtos = submissions.Select(s => new SubmissionDto
            {
                ItemId = s.ItemId ?? Guid.Empty,
                Title = s.Item?.Title ?? "(Untitled)",
                Description = s.Item?.Description ?? string.Empty,
                SubmittedBy = s.Item?.Owner?.Name ?? "Unknown",
                CreatedBy = s.Item?.CreatedBy,
                Tags = s.Item?.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                Attachments = s.Item?.Attachments?.Select(a => new FileAttachmentDto
                {
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    MimeType = a.MimeType,
                    FileSize = a.FileSize ?? 0
                }).ToList() ?? new List<FileAttachmentDto>(),
                CreatedOn = s.CreatedOn
            }).ToList();

            return new UserEventInsightDto
            {
                EventId = eventId,
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                AverageRating = avgRating,
                TeamMembers = teamMemberDtos,
                Submissions = submissionDtos,
             
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
                    ItemId = s.ItemId ?? Guid.Empty,
                    Title = s.Item?.Title ?? "(Untitled)",
                    Description = s.Item?.Description ?? "",
                    SubmittedBy = s.Item?.Owner?.Name ?? "Unknown",
                    CreatedBy = s.Item?.CreatedBy,
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

            }).ToList();
        }
    }
}