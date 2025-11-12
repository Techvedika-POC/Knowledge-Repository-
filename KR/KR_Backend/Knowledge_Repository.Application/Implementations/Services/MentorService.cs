using Knowledge_Repository.Application.Dtos.Mentor;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
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

            return teams.Select(t => new TeamDetailsDto
            {
                TeamId = t.TeamId,
                TeamName = t.TeamName,
                EventId = t.EventId,
                Description = t.Event?.Description, 
                ProjectTitle = null
            }).ToList();
        }


        public async Task<TeamDetailsDto> GetTeamDetailsAsync(Guid teamId)
        {
            if (teamId == Guid.Empty)
                throw new ArgumentException("Invalid team ID.", nameof(teamId));

            var team = await _mentorRepo.GetTeamDetailsAsync(teamId);
            if (team == null)
                throw new KeyNotFoundException("Team not found.");

            var feedbacks = await _mentorRepo.GetTeamFeedbacksAsync(teamId);

            // Safely map members
            var members = team.TeamMembers?.Select(m => new MemberDto
            {
                UserId = m.UserId,
                Name = m.User?.Name, 
                Email = m.User?.Email,
                Role = m.User?.UserRoleUsers
                             ?.FirstOrDefault()?.Role?.RoleName 
            }).ToList() ?? new List<MemberDto>();

            return new TeamDetailsDto
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                EventId = team.EventId, 
                Description = team.Event?.Description, 
                ProjectTitle = null,
                Members = members,
                Feedbacks = feedbacks.Select(f => new FeedbackResponseDto
                {
                    FeedbackId = f.FeedbackId,
                    MentorId = f.MentorId,
                    TeamId = f.TeamId,
                    FeedbackText = f.FeedbackText,
                    ProgressRating = f.ProgressRating,
                    CreatedOn = f.CreatedOn
                }).ToList()
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
