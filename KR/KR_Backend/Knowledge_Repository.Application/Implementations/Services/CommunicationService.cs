using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Dtos.CommunicationBetweenMentorAndTeam;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
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
            _repo = repo;
            _teamMemberRepo = teamMemberRepo;
            _mentorRepo = mentorRepo;
            _userRepo = userRepo;
        }

        private async Task<bool> IsUserAllowedOnTeam(Guid userId, Guid teamId)
        {
            if (userId == Guid.Empty) return false;
            var isMember = await _teamMemberRepo.IsUserInTeamAsync(teamId, userId);
            if (isMember) return true;
            var assigned = await _mentorRepo.GetAssignedTeamsAsync(userId);
            if (assigned != null && assigned.Any(t => t.TeamId == teamId)) return true;

            return false;
        }

   
        public async Task<IEnumerable<ChatMessagesDto>> GetTeamChatMessagesAsync(Guid userId, Guid teamId)
        {
            if (!await IsUserAllowedOnTeam(userId, teamId))
                throw new SecurityException("User not allowed to view messages for this team.");

            var msgs = await _repo.GetTeamChatMessagesAsync(teamId);
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

        public async Task<ChatMessagesDto> PostTeamChatMessageAsync(Guid userId, Guid teamId, string messageText, string? senderName = null)
        {
            if (!await IsUserAllowedOnTeam(userId, teamId))
                throw new SecurityException("User not allowed to post messages for this team.");

            var uname = senderName;
            if (string.IsNullOrEmpty(uname))
            {
                uname = await _userRepo.GetUserNameAsync(userId) ?? "Unknown";
            }

            var msg = new TeamChatMessage
            {
                TeamId = teamId,
                SenderId = userId,
                SenderName = uname,
                MessageText = messageText,
                CreatedOn = DateTime.UtcNow
            };

            var saved = await _repo.AddTeamChatMessageAsync(msg);

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

        public async Task<bool> SoftDeleteChatMessageAsync(Guid userId, Guid messageId)
        {
            var msg = await _repo.GetChatMessageByIdAsync(messageId);
            if (msg == null) return false;

       
            if (msg.SenderId != userId)
            {
            
                var isMentorForTeam = (await _mentorRepo.GetAssignedTeamsAsync(userId))?.Any(t => t.TeamId == msg.TeamId) ?? false;
                if (!isMentorForTeam) throw new SecurityException("Not authorized to delete this message.");
            }

            return await _repo.SoftDeleteChatMessageAsync(messageId);
        }
        public async Task<IEnumerable<FeedbackDto>> GetTeamFeedbacksAsync(Guid userId, Guid teamId)
        {
            if (!await IsUserAllowedOnTeam(userId, teamId))
                throw new SecurityException("User not allowed.");

            var fbs = await _repo.GetTeamFeedbacksAsync(teamId);
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

        public async Task<FeedbackDto> CreateFeedbackAsync(FeedbackCreateRequest req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (req.TeamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(req.TeamId));

                Mentor? mentor = null;
            if (req.MentorId != Guid.Empty)
            {
                mentor = await _mentorRepo.GetByIdAsync(req.MentorId);
            }

            var userId = req.RequestingUserId;

        
            if (mentor == null && userId != Guid.Empty)
            {
                mentor = await _mentorRepo.GetByUserAndTeamAsync(userId, req.TeamId);
            }

         
            if (mentor == null && userId != Guid.Empty && req.EventId != Guid.Empty)
            {
                mentor = await _mentorRepo.GetByUserAndEventAsync(userId, req.EventId);
            }

            if (mentor == null && userId != Guid.Empty)
            {
                var mentors = (await _mentorRepo.GetByUserAsync(userId)).ToList();
                if (mentors.Count == 1)
                {
                    mentor = mentors.Single();
                }
                else if (mentors.Count > 1)
                {

                    mentor = mentors.FirstOrDefault(m => m.AssignedTeamId == req.TeamId)
                             ?? mentors.FirstOrDefault(m => m.EventId == req.EventId);
                    if (mentor == null)
                    {
                        throw new SecurityException("Multiple mentor records found for the current user. Please send a request that includes the correct teamId or eventId so the server can resolve which mentor record to use.");
                    }
                }
            }

            if (mentor == null)
                throw new SecurityException("Current user is not registered as a mentor for this team. Please contact admin.");

    
            var f = new TeamFeedback
            {
                FeedbackId = Guid.NewGuid(),
                MentorId = mentor.MentorId,
                TeamId = req.TeamId,
                EventId = req.EventId,
                FeedbackText = req.FeedbackText,
                ProgressRating = req.ProgressRating,
                CreatedOn = DateTime.UtcNow
            };

            var saved = await _repo.AddTeamFeedbackAsync(f);

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


        public async Task<FeedbackReplyDto> CreateFeedbackReplyAsync(FeedbackReplyCreateRequest req)
        {
            if (!await IsUserAllowedOnTeam(req.UserId, req.TeamId))
                throw new SecurityException("User not allowed.");

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

            var uname = await _userRepo.GetUserNameAsync(req.UserId);

            return new FeedbackReplyDto
            {
                ReplyId = saved.ReplyId,
                FeedbackId = saved.FeedbackId,
                TeamId = saved.TeamId,
                UserId = saved.UserId,
                ReplyText = saved.ReplyText,
                CreatedOn = saved.CreatedOn,
                UserName = uname
            };
        }
    }
}
