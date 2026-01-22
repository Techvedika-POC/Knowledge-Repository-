using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Dtos.CommunicationBetweenMentorAndTeam;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System.Security;

namespace Knowledge_Repository.Application.Implementations.Services
{
    /// <summary>
    /// Handles team communication including chat messages and mentor feedback.
    /// Ensures proper authorization for team members and mentors.
    /// </summary>
    public class CommunicationService : ICommunicationService
    {
        private readonly ICommunicationRepository _repo;
        private readonly ITeamMemberRepository _teamMemberRepo;
        private readonly IMentorRepository _mentorRepo;
        private readonly IUserRepository _userRepo;

        public CommunicationService(
            ICommunicationRepository repo,
            ITeamMemberRepository teamMemberRepo,
            IMentorRepository mentorRepo,
            IUserRepository userRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _teamMemberRepo = teamMemberRepo ?? throw new ArgumentNullException(nameof(teamMemberRepo));
            _mentorRepo = mentorRepo ?? throw new ArgumentNullException(nameof(mentorRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        /// <summary>
        /// Determines whether a user is authorized to access a team
        /// (either as a team member or an assigned mentor).
        /// </summary>
        private async Task<bool> IsUserAllowedOnTeam(Guid userId, Guid teamId)
        {
            if (userId == Guid.Empty || teamId == Guid.Empty)
                return false;

            if (await _teamMemberRepo.IsUserInTeamAsync(teamId, userId))
                return true;

            var assignedTeams = await _mentorRepo.GetAssignedTeamsAsync(userId);
            return assignedTeams?.Any(t => t.TeamId == teamId) == true;
        }

        /// <summary>
        /// Retrieves chat messages for a team if the user is authorized.
        /// </summary>
        public async Task<IEnumerable<ChatMessagesDto>> GetTeamChatMessagesAsync(Guid userId, Guid teamId)
        {
            if (!await IsUserAllowedOnTeam(userId, teamId))
                throw new SecurityException(
                    "Access denied. You are not authorized to view messages for this team.");

            var msgs = await _repo.GetTeamChatMessagesAsync(teamId)
                ?? throw new InvalidOperationException(
                    "Unable to retrieve team messages at this time.");

            return msgs.Select(m => new ChatMessagesDto
            {
                MessageId = m.MessageId,
                TeamId = m.TeamId,
                SenderId = m.SenderId,
                SenderName = m.SenderName,
                MessageText = m.MessageText,
                CreatedOn = m.CreatedOn,
                EditedOn = m.EditedOn,
                IsDeleted = m.IsDeleted
            });
        }

        /// <summary>
        /// Posts a new chat message to a team.
        /// </summary>
        public async Task<ChatMessagesDto> PostTeamChatMessageAsync(
            Guid userId,
            Guid teamId,
            string messageText,
            string? senderName = null)
        {
            if (!await IsUserAllowedOnTeam(userId, teamId))
                throw new SecurityException(
                    "Access denied. You are not authorized to post messages to this team.");

            if (string.IsNullOrWhiteSpace(messageText))
                throw new ArgumentException(
                    "Message text cannot be empty.",
                    nameof(messageText));

            var resolvedName = senderName;
            if (string.IsNullOrWhiteSpace(resolvedName))
                resolvedName = await _userRepo.GetUserNameAsync(userId) ?? "Unknown";

            var msg = new TeamChatMessage
            {
                TeamId = teamId,
                SenderId = userId,
                SenderName = resolvedName,
                MessageText = messageText,
                CreatedOn = DateTime.UtcNow
            };

            var saved = await _repo.AddTeamChatMessageAsync(msg)
                ?? throw new InvalidOperationException(
                    "Failed to save the chat message.");

            return new ChatMessagesDto
            {
                MessageId = saved.MessageId,
                TeamId = saved.TeamId,
                SenderId = saved.SenderId,
                SenderName = saved.SenderName,
                MessageText = saved.MessageText,
                CreatedOn = saved.CreatedOn
            };
        }

        /// <summary>
        /// Soft-deletes a chat message.
        /// Only the sender or an assigned mentor can delete a message.
        /// </summary>
        public async Task<bool> SoftDeleteChatMessageAsync(Guid userId, Guid messageId)
        {
            if (messageId == Guid.Empty)
                throw new ArgumentException(
                    "Message ID cannot be empty.",
                    nameof(messageId));

            var msg = await _repo.GetChatMessageByIdAsync(messageId);

            if (msg == null)
                throw new KeyNotFoundException(
                    $"No chat message found with ID '{messageId}'.");

            if (msg.SenderId != userId)
            {
                var isMentorForTeam =
                    (await _mentorRepo.GetAssignedTeamsAsync(userId))
                    ?.Any(t => t.TeamId == msg.TeamId) == true;

                if (!isMentorForTeam)
                    throw new SecurityException(
                        "You are not authorized to delete this message.");
            }

            return await _repo.SoftDeleteChatMessageAsync(messageId);
        }

        /// <summary>
        /// Retrieves mentor feedbacks for a team.
        /// </summary>
        public async Task<IEnumerable<FeedbackDto>> GetTeamFeedbacksAsync(Guid userId, Guid teamId)
        {
            if (!await IsUserAllowedOnTeam(userId, teamId))
                throw new SecurityException(
                    "Access denied. You are not authorized to view feedback for this team.");

            var fbs = await _repo.GetTeamFeedbacksAsync(teamId)
                ?? throw new InvalidOperationException(
                    "Unable to retrieve feedbacks at this time.");

            return fbs.Select(f => new FeedbackDto
            {
                FeedbackId = f.FeedbackId,
                MentorId = f.MentorId,
                TeamId = f.TeamId,
                EventId = f.EventId,
                FeedbackText = f.FeedbackText,
                ProgressRating = f.ProgressRating,
                CreatedOn = f.CreatedOn,
                UpdatedOn = f.UpdatedOn,
                LastReplyOn = f.LastReplyOn,
                Replies = f.TeamFeedbackReplies?
                    .Where(r => !r.IsDeleted)
                    .OrderBy(r => r.CreatedOn)
                    .Select(r => new FeedbackReplyDto
                    {
                        ReplyId = r.ReplyId,
                        FeedbackId = r.FeedbackId,
                        TeamId = r.TeamId,
                        UserId = r.UserId,
                        ReplyText = r.ReplyText,
                        CreatedOn = r.CreatedOn,
                        EditedOn = r.EditedOn,
                        IsDeleted = r.IsDeleted,
                        UserName = r.User?.Name
                    }).ToList() ?? new List<FeedbackReplyDto>()
            });
        }

        /// <summary>
        /// Creates mentor feedback for a team.
        /// Automatically resolves the correct mentor record.
        /// </summary>
        public async Task<FeedbackDto> CreateFeedbackAsync(FeedbackCreateRequest req)
        {
            if (req == null)
                throw new ArgumentNullException(nameof(req));

            if (req.TeamId == Guid.Empty)
                throw new ArgumentException(
                    "Team ID is required.",
                    nameof(req.TeamId));

            Mentor? mentor = null;

            if (req.MentorId != Guid.Empty)
                mentor = await _mentorRepo.GetByIdAsync(req.MentorId);

            if (mentor == null && req.RequestingUserId != Guid.Empty)
                mentor = await _mentorRepo.GetByUserAndTeamAsync(req.RequestingUserId, req.TeamId)
                      ?? await _mentorRepo.GetByUserAndEventAsync(req.RequestingUserId, req.EventId);

            if (mentor == null && req.RequestingUserId != Guid.Empty)
            {
                var mentors = (await _mentorRepo.GetByUserAsync(req.RequestingUserId)).ToList();

                if (mentors.Count == 1)
                    mentor = mentors.Single();
                else if (mentors.Count > 1)
                    mentor = mentors.FirstOrDefault(m => m.AssignedTeamId == req.TeamId)
                          ?? mentors.FirstOrDefault(m => m.EventId == req.EventId)
                          ?? throw new SecurityException(
                              "Multiple mentor records found. Please specify teamId or eventId.");
            }

            if (mentor == null)
                throw new SecurityException(
                    "You are not registered as a mentor for this team.");

            var feedback = new TeamFeedback
            {
                FeedbackId = Guid.NewGuid(),
                MentorId = mentor.MentorId,
                TeamId = req.TeamId,
                EventId = req.EventId,
                FeedbackText = req.FeedbackText,
                ProgressRating = req.ProgressRating,
                CreatedOn = DateTime.UtcNow
            };

            var saved = await _repo.AddTeamFeedbackAsync(feedback);

            return new FeedbackDto
            {
                FeedbackId = saved.FeedbackId,
                MentorId = saved.MentorId,
                TeamId = saved.TeamId,
                EventId = saved.EventId,
                FeedbackText = saved.FeedbackText,
                ProgressRating = saved.ProgressRating,
                CreatedOn = saved.CreatedOn
            };
        }

        /// <summary>
        /// Adds a reply to an existing feedback thread.
        /// </summary>
        public async Task<FeedbackReplyDto> CreateFeedbackReplyAsync(FeedbackReplyCreateRequest req)
        {
            if (req == null)
                throw new ArgumentNullException(nameof(req));

            if (!await IsUserAllowedOnTeam(req.UserId, req.TeamId))
                throw new SecurityException(
                    "Access denied. You are not authorized to reply to this feedback.");

            var reply = new TeamFeedbackReply
            {
                ReplyId = Guid.NewGuid(),
                FeedbackId = req.FeedbackId,
                TeamId = req.TeamId,
                UserId = req.UserId,
                ReplyText = req.ReplyText,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            var saved = await _repo.AddTeamFeedbackReplyAsync(reply);

            return new FeedbackReplyDto
            {
                ReplyId = saved.ReplyId,
                FeedbackId = saved.FeedbackId,
                TeamId = saved.TeamId,
                UserId = saved.UserId,
                ReplyText = saved.ReplyText,
                CreatedOn = saved.CreatedOn,
                UserName = await _userRepo.GetUserNameAsync(req.UserId)
            };
        }
    }
}
