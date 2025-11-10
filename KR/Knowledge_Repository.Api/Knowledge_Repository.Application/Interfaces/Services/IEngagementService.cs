using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IEngagementService
    {
        /// <summary>
        /// Add a new engagement (Like, Favourite, Comment)
        /// </summary>
        Task AddEngagementAsync(EngagementDto dto);

        /// <summary>
        /// Remove an engagement for a specific item by a user
        /// </summary>
        Task RemoveEngagementAsync(Guid itemId, Guid userId, string engagementType);

        /// <summary>
        /// Get summary of engagements (Likes, Favourites, Comments) for an item
        /// </summary>
        Task<KnowledgeItemEngagementDto> GetEngagementSummaryAsync(Guid itemId, Guid userId);

        /// <summary>
        /// Get all engagements for a particular user (Likes and Favourites)
        /// </summary>
        Task<List<UserEngagementDto>> GetUserEngagementsAsync(Guid userId);

        /// <summary>
        /// Get the total likes count for a specific item
        /// </summary>
        Task<int> GetLikesCountAsync(Guid itemId);

        /// <summary>
        /// Get the top liked items across the platform, limited by top N
        /// </summary>
        Task<List<LeaderboardDto>> GetTopLikedItemsAsync(int top = 5);
    }
}
