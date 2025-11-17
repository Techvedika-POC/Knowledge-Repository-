using Knowledge_Repository.Application.Dtos.Mentor;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Application.Dtos.EventInsight;

using Knowledge_Repository.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class MentorService : IMentorService
    {
        private readonly IMentorRepository _mentorRepo;

        public MentorService(IMentorRepository mentorRepo)
        {
            _mentorRepo = mentorRepo;
        }
        public async Task<IEnumerable<TeamDetailsDto>> GetTeamsForMentorAsync(Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                throw new ArgumentException("Invalid mentor ID.", nameof(mentorId));

            var teams = await _mentorRepo.GetAssignedTeamsAsync(mentorId);

            var teamDtos = new List<TeamDetailsDto>();

            foreach (var team in teams.Where(t => t != null))
            {
                var eventDescription = team.Event?.Description ?? "No description available";

                teamDtos.Add(new TeamDetailsDto
                {
                    TeamId = team.TeamId,
                    TeamName = team.TeamName,

                    EventId = team.EventId ?? Guid.Empty,

                    Description = eventDescription,
                    ProjectTitle = null
                });
            }

            return teamDtos;
        }


        public async Task<TeamDetailsDto> GetTeamDetailsAsync(Guid teamId)
        {
            if (teamId == Guid.Empty)
                throw new ArgumentException("Invalid team ID.", nameof(teamId));

            var team = await _mentorRepo.GetTeamDetailsAsync(teamId);
            if (team == null)
                throw new KeyNotFoundException("Team not found.");

            var feedbacks = await _mentorRepo.GetTeamFeedbacksAsync(teamId);

            var members = team.TeamMembers?.Select(m => new MemberDto
            {
                UserId = m.UserId,
                Name = m.User?.Name,
                Email = m.User?.Email,
                Role = m.User?.UserRoleUsers?.FirstOrDefault()?.Role?.RoleName
            }).ToList() ?? new List<MemberDto>();


            var submissions = team.EventKnowledgeItems
                .Where(eki => eki.Item != null)
                .Select(eki => eki.Item)
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,

                    Tags = k.KnowledgeTags
                            .Select(t => t.TagName)
                            .ToList(),

                    DomainId = k.DomainId,
                    DomainName = k.Domain?.DomainName,
                    CategoryId = k.CategoryId,
                    CategoryName = k.Category?.CategoryName,

                    OwnerId = k.OwnerId,
                    OwnerName = k.Owner?.Name,

                    Views = k.Engagements.Count(e => e.EngagementType == "View"),
                    Likes = k.Engagements.Count(e => e.EngagementType == "Like"),
                    Comments = k.Engagements.Count(e => e.EngagementType == "Comment"),
                    EngagementScore = k.Engagements.Count(),

                    IsEventItem = k.IsEventItem,
                    CreatedBy = k.CreatedBy,
                    CreatedByName = k.CreatedByNavigation?.Name,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedByName = k.UpdatedByNavigation?.Name,

                    CreatedOn = k.CreatedOn.HasValue
                        ? new DateTimeOffset(k.CreatedOn.Value)
                        : default,

                    UpdatedOn = k.UpdatedOn,

                    Language = k.Language,
                    Framework = k.Framework,
                    Metadata = k.Metadata,

                    KnowledgeItem = k.KnowledgeText,
                    SubmittedBy = k.Owner?.Name
                })
                .ToList();


            var feedbackDtos = feedbacks.Select(f => new FeedbackResponseDto
            {
                FeedbackId = f.FeedbackId,
                MentorId = f.MentorId,
                TeamId = f.TeamId,
                FeedbackText = f.FeedbackText,
                ProgressRating = f.ProgressRating,
                CreatedOn = f.CreatedOn,

                Replies = team.TeamFeedbackReplies
                    .Where(r => r.FeedbackId == f.FeedbackId)
                    .Select(r => new FeedbackReplyDto
                    {
                        ReplyId = r.ReplyId,
                        ReplyText = r.ReplyText,
                        UserName = r.User?.Name,
                        CreatedOn = r.CreatedOn
                    })
                    .ToList()
            }).ToList();


            return new TeamDetailsDto
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                EventId = team.EventId ?? Guid.Empty,
                Description = team.Event?.Description,
                ProjectTitle = null,
                Members = members,
                Feedbacks = feedbackDtos,
                Submissions = submissions
            };
        }


        public async Task<FeedbackResponseDto> AddFeedbackAsync(AddFeedbackRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.TeamId == Guid.Empty)
                throw new ArgumentException("Invalid team ID.", nameof(request.TeamId));
            if (request.EventId == Guid.Empty)
                throw new ArgumentException("Invalid event ID.", nameof(request.EventId));
            if (string.IsNullOrWhiteSpace(request.FeedbackText))
                throw new ArgumentException("Feedback text cannot be empty.", nameof(request.FeedbackText));

            var mentor = await _mentorRepo.GetMentorByUserIdAsync(request.MentorId);
            if (mentor == null)
                throw new KeyNotFoundException("Mentor record not found for the given user ID.");

            var feedback = new TeamFeedback
            {
                FeedbackId = Guid.NewGuid(),
                MentorId = mentor.MentorId,
                TeamId = request.TeamId,
                EventId = request.EventId,
                FeedbackText = request.FeedbackText.Trim(),
                ProgressRating = request.ProgressRating,
                CreatedOn = DateTime.UtcNow
            };

            await _mentorRepo.AddFeedbackAsync(feedback);

            return new FeedbackResponseDto
            {
                FeedbackId = feedback.FeedbackId,
                MentorId = mentor.MentorId,
                TeamId = feedback.TeamId,
                FeedbackText = feedback.FeedbackText,
                ProgressRating = feedback.ProgressRating,
                CreatedOn = feedback.CreatedOn
            };
        }

        public async Task<bool> UpdateFeedbackAsync(UpdateFeedbackRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.FeedbackId == Guid.Empty)
                throw new ArgumentException("Invalid feedback ID.", nameof(request.FeedbackId));
            if (string.IsNullOrWhiteSpace(request.FeedbackText))
                throw new ArgumentException("Feedback text cannot be empty.", nameof(request.FeedbackText));

            var feedback = await _mentorRepo.GetFeedbackByIdAsync(request.FeedbackId);
            if (feedback == null)
                return false;

            feedback.FeedbackText = request.FeedbackText.Trim();
            feedback.ProgressRating = request.ProgressRating;
            feedback.UpdatedOn = DateTime.UtcNow;

            await _mentorRepo.UpdateFeedbackAsync(feedback);
            return true;
        }

        public async Task<IEnumerable<FeedbackResponseDto>> GetFeedbacksByMentorAsync(Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                throw new ArgumentException("Invalid mentor ID.", nameof(mentorId));

            var feedbacks = await _mentorRepo.GetFeedbacksByMentorAsync(mentorId);
            return feedbacks.Select(f => new FeedbackResponseDto
            {
                FeedbackId = f.FeedbackId,
                MentorId = f.MentorId,
                TeamId = f.TeamId,
                FeedbackText = f.FeedbackText,
                ProgressRating = f.ProgressRating,
                CreatedOn = f.CreatedOn
            }).ToList();
        }

        public async Task<FeedbackResponseDto> GetFeedbackByIdAsync(Guid feedbackId)
        {
            if (feedbackId == Guid.Empty)
                throw new ArgumentException("Invalid feedback ID.", nameof(feedbackId));

            var feedback = await _mentorRepo.GetFeedbackByIdAsync(feedbackId);
            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found.");

            return new FeedbackResponseDto
            {
                FeedbackId = feedback.FeedbackId,
                MentorId = feedback.MentorId,
                TeamId = feedback.TeamId,
                FeedbackText = feedback.FeedbackText,
                ProgressRating = feedback.ProgressRating,
                CreatedOn = feedback.CreatedOn
            };
        }
    }
}
