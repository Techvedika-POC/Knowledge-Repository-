using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class EngagementService : IEngagementService
    {
        private readonly IEngagementRepository _engagementRepository;
        private readonly IKnowledgeItemRepository _knowledgeItemRepository;
        private readonly IUserRepository _userRepository;

        public EngagementService(
            IEngagementRepository engagementRepository,
            IKnowledgeItemRepository knowledgeItemRepository,
            IUserRepository userRepository)
        {
            _engagementRepository = engagementRepository;
            _knowledgeItemRepository = knowledgeItemRepository;
            _userRepository = userRepository;
        }

        public async Task AddEngagementAsync(EngagementDto dto)
        {
            var engagement = new Engagement
            {
                EngagementId = Guid.NewGuid(),
                ItemId = dto.ItemId,
                UserId = dto.UserId,
                EngagementType = dto.EngagementType,
                CommentText = dto.CommentText ?? string.Empty,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                Points = dto.EngagementType switch
                {
                    "Like" => 1,
                    "Favourite" => 1,
                    _ => 0
                }
            };

            await _engagementRepository.AddAsync(engagement);
        }

        public async Task RemoveEngagementAsync(Guid itemId, Guid userId, string engagementType)
        {
            var engagement = await _engagementRepository.FirstOrDefaultAsync(
                e => e.ItemId == itemId && e.UserId == userId && e.EngagementType == engagementType);

            if (engagement != null)
                await _engagementRepository.DeleteAsync(engagement);
        }

        public async Task<int> GetLikesCountAsync(Guid itemId)
        {
            return await _engagementRepository.CountAsync(
                e => e.ItemId == itemId && e.EngagementType == "Like");
        }
        public async Task UpdateCommentAsync(Guid engagementId, string newText)
        {
            var comment = await _engagementRepository.GetByIdAsync(engagementId);
            if (comment == null) throw new Exception("Comment not found");

            comment.CommentText = newText;
            comment.UpdatedOn = DateTime.UtcNow;

            await _engagementRepository.UpdateAsync(comment);
        }


        public async Task<List<UserEngagementDto>> GetUserEngagementsAsync(Guid userId)
        {
            var engagements = await _engagementRepository.GetAllAsync(
                e => e.UserId == userId && (e.EngagementType == "Like" || e.EngagementType == "Favourite"));

            return engagements.Select(e => new UserEngagementDto
            {
                ItemId = e.ItemId,
                EngagementType = e.EngagementType
            }).ToList();
        }

     

        public async Task<KnowledgeItemEngagementDto> GetEngagementSummaryAsync(Guid itemId, Guid userId)
        {
            var engagements = await _engagementRepository.GetAllAsync(e => e.ItemId == itemId);

            var likesCount = engagements.Count(e => e.EngagementType == "Like");
            var favouritesCount = engagements.Count(e => e.EngagementType == "Favourite");

            var comments = engagements
                .Where(e => e.EngagementType == "Comment" && e.ParentCommentId == null)
                .Select(MapToCommentDto)
                .ToList();

            var userEngagementTypes = engagements
                .Where(e => e.UserId == userId)
                .Select(e => e.EngagementType)
                .ToList();

            return new KnowledgeItemEngagementDto
            {
                ItemId = itemId,
                LikesCount = likesCount,
                FavouritesCount = favouritesCount,
                CommentsCount = comments.Count,
                Comments = comments,
                UserEngagementTypes = userEngagementTypes
            };
        }

 

        public async Task<List<CommentDto>> GetCommentsByItemAsync(Guid itemId)
        {
            var comments = await _engagementRepository.GetCommentsByItemAsync(itemId);
            return comments.Select(MapToCommentDto).ToList();
        }

        public async Task<List<CommentDto>> GetRepliesAsync(Guid parentCommentId)
        {
            var replies = await _engagementRepository.GetRepliesAsync(parentCommentId);
            return replies.Select(MapToCommentDto).ToList();
        }

        public async Task AddCommentAsync(CommentDto dto)
        {
            var comment = new Engagement
            {
                EngagementId = Guid.NewGuid(),
                ItemId = dto.ItemId,
                UserId = dto.UserId,
                EngagementType = "Comment",
                CommentText = dto.CommentText,
                ParentCommentId = dto.ParentCommentId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

            await _engagementRepository.AddAsync(comment);
        }

        public async Task DeleteCommentAsync(Guid engagementId)
        {
            var comment = await _engagementRepository.GetByIdAsync(engagementId);
            if (comment != null)
                await _engagementRepository.DeleteAsync(comment);
        }


        public async Task<List<UserLeaderboardDto>> GetTopUsersByLikesAsync(int top = 3)
        {
            // 1. Fetch all LIKE engagements
            var likes = await _engagementRepository.GetAllAsync(
                e => e.EngagementType == "Like"
            );

            if (!likes.Any())
                return new List<UserLeaderboardDto>();

            // 2. Get all liked item IDs
            var itemIds = likes.Select(l => l.ItemId).Distinct().ToList();

            // 3. Load items with their owners
            var items = await _knowledgeItemRepository.GetByItemIdsAsync(itemIds);

            // 4. Group likes by ITEM OWNER
            var ownerLikeGroups = likes
                .Join(items,
                      like => like.ItemId,
                      item => item.ItemId,
                      (like, item) => new { item.OwnerId })
                .Where(x => x.OwnerId != null)
                .GroupBy(x => x.OwnerId)
                .Select(g => new
                {
                    UserId = g.Key.Value,
                    TotalLikes = g.Count()
                })
                .OrderByDescending(x => x.TotalLikes)
                .Take(top)
                .ToList();

            // 5. Get full user details
            var userIds = ownerLikeGroups.Select(x => x.UserId).ToList();
            var users = await _userRepository.GetUsersByIdsAsync(userIds);

            // 6. Build final leaderboard DTO
            var leaderboard = ownerLikeGroups.Select(g =>
            {
                var user = users.FirstOrDefault(u => u.UserId == g.UserId);

                return new UserLeaderboardDto
                {
                    UserId = g.UserId,
                    UserName = user?.Name ?? "Unknown",
                    Department = user?.Department?.DepartmentName ?? "N/A",
                    TotalLikesReceived = g.TotalLikes
                };
            })
            .OrderByDescending(x => x.TotalLikesReceived)
            .ToList();

            return leaderboard;
        }
        private static CommentDto MapToCommentDto(Engagement e)
        {
            return new CommentDto
            {
                EngagementId = e.EngagementId,
                ItemId = e.ItemId,
                UserId = e.UserId,
                UserName = e.User?.Name ?? "Unknown",
                CommentText = e.CommentText ?? string.Empty,
                CreatedOn = e.CreatedOn,
                UpdatedOn = e.UpdatedOn,
                ParentCommentId = e.ParentCommentId,
                Replies = e.InverseParentComment?
                    .Where(r => r.EngagementType == "Comment")
                    .Select(MapToCommentDto)
                    .ToList() ?? new List<CommentDto>()
            };
        }

    }
}
