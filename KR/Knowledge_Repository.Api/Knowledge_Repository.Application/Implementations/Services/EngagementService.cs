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
            var engagement = await _engagementRepository
                .FirstOrDefaultAsync(e => e.ItemId == itemId && e.UserId == userId && e.EngagementType == engagementType);

            if (engagement != null)
                await _engagementRepository.DeleteAsync(engagement);
        }

        public async Task<KnowledgeItemEngagementDto> GetEngagementSummaryAsync(Guid itemId, Guid userId)
        {
            var engagements = await _engagementRepository
                .GetAllAsync(e => e.ItemId == itemId);

            var likesCount = engagements.Count(e => e.EngagementType == "Like");
            var favouritesCount = engagements.Count(e => e.EngagementType == "Favourite");

            var comments = engagements
                .Where(e => e.EngagementType == "Comment")
                .Select(e => new CommentDto
                {
                    EngagementId = e.EngagementId,
                    UserId = e.UserId,
                    UserName = e.User?.Name ?? "Unknown",
                    CommentText = e.CommentText ?? string.Empty,
                    CreatedOn = e.CreatedOn
                })
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

        public async Task<int> GetLikesCountAsync(Guid itemId)
        {
            return await _engagementRepository.CountAsync(e =>
                e.ItemId == itemId && e.EngagementType == "Like");
        }

        public async Task<List<UserEngagementDto>> GetUserEngagementsAsync(Guid userId)
        {
            var engagements = await _engagementRepository
                .GetAllAsync(e => e.UserId == userId && (e.EngagementType == "Like" || e.EngagementType == "Favourite"));

            return engagements
                .Select(e => new UserEngagementDto
                {
                    ItemId = e.ItemId,
                    EngagementType = e.EngagementType
                })
                .ToList();
        }

        public async Task<List<LeaderboardDto>> GetTopLikedItemsAsync(int top = 5)
        {
          
            var likesOnly = await _engagementRepository.GetAllAsync(e => e.EngagementType == "Like");

            var groupedLikes = likesOnly
                .GroupBy(e => e.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    LikesCount = g.Count()
                })
                .OrderByDescending(x => x.LikesCount)
                .Take(top)
                .ToList();

            var itemIds = groupedLikes.Select(t => t.ItemId).ToList();

            var items = await _knowledgeItemRepository.GetByItemIdsAsync(itemIds);

            var leaderboard = items
                .Select(k =>
                {
                    var likes = groupedLikes.FirstOrDefault(t => t.ItemId == k.ItemId)?.LikesCount ?? 0;
                    return new LeaderboardDto
                    {
                        ItemId = k.ItemId,
                        ItemTitle = k.Title,
                        ItemDescription = string.IsNullOrEmpty(k.Description)
                            ? ""
                            : (k.Description.Length > 100 ? k.Description.Substring(0, 100) + "..." : k.Description),
                        UserId = k.OwnerId ?? Guid.Empty,
                        UserName = k.Owner?.Name ?? "Unknown",
                        LikesCount = likes
                    };
                })
                .OrderByDescending(l => l.LikesCount)
                .ToList();

            return leaderboard;
        }




    }
}
