using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowLedger_Synaptix.Services.Implementations
{
    /// <summary>
    /// Service to handle engagements on knowledge items such as Likes, Favourites, and Comments.
    /// </summary>
    public class EngagementService : IEngagementService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EngagementService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds an engagement (Like, Favourite, Comment) for a user on a knowledge item.
        /// Points are automatically assigned based on the engagement type.
        /// </summary>
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

            _context.Engagements.Add(engagement);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a specific engagement for a user on a knowledge item.
        /// </summary>
        public async Task RemoveEngagementAsync(Guid itemId, Guid userId, string engagementType)
        {
            var engagement = await _context.Engagements
                .FirstOrDefaultAsync(e =>
                    e.ItemId == itemId &&
                    e.UserId == userId &&
                    e.EngagementType == engagementType);

            if (engagement != null)
            {
                _context.Engagements.Remove(engagement);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Returns a detailed engagement summary for a knowledge item, including:
        /// - Total likes
        /// - Total favourites
        /// - Total comments and comment details
        /// - Engagement types of the requesting user
        /// </summary>
        public async Task<KnowledgeItemEngagementDto> GetEngagementSummaryAsync(Guid itemId, Guid userId)
        {
            // Count total likes
            var likesCount = await _context.Engagements
                .CountAsync(e => e.ItemId == itemId && e.EngagementType == "Like");

            // Count total favourites
            var favouritesCount = await _context.Engagements
                .CountAsync(e => e.ItemId == itemId && e.EngagementType == "Favourite");

            // Fetch all comments with user info
            var comments = (await _context.Engagements
                .Where(e => e.ItemId == itemId && e.EngagementType == "Comment")
                .Include(e => e.User)
                .ToListAsync())
                .Select(e => new CommentDto
                {
                    EngagementId = e.EngagementId,
                    UserId = e.UserId,
                    UserName = e.User?.Name ?? "Unknown",
                    CommentText = e.CommentText ?? "",
                    CreatedOn = e.CreatedOn
                })
                .ToList();

            // Determine the engagement types of the current user
            var userEngagementTypes = await _context.Engagements
                .Where(e => e.ItemId == itemId && e.UserId == userId)
                .Select(e => e.EngagementType)
                .ToListAsync();

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

        /// <summary>
        /// Returns the total number of likes for a specific knowledge item.
        /// </summary>
        public async Task<int> GetLikesCountAsync(Guid itemId)
        {
            return await _context.Engagements
                .CountAsync(e => e.ItemId == itemId && e.EngagementType == "Like");
        }

        /// <summary>
        /// Returns a list of all engagements (Likes and Favourites) performed by a user.
        /// </summary>
        public async Task<List<UserEngagementDto>> GetUserEngagementsAsync(Guid userId)
        {
            return await _context.Engagements
                .Where(e => e.UserId == userId && (e.EngagementType == "Like" || e.EngagementType == "Favourite"))
                .Select(e => new UserEngagementDto
                {
                    ItemId = e.ItemId,
                    EngagementType = e.EngagementType
                })
                .ToListAsync();
        }
        public async Task<List<LeaderboardDto>> GetTopLikedItemsAsync(int top = 5)
        {
            var topItems = await _context.Engagements
             .Where(e => e.EngagementType == "Like")
             .GroupBy(e => e.ItemId)
             .Select(g => new
             {
                 ItemId = g.Key,
                 LikesCount = g.Count()
             })
             .OrderByDescending(x => x.LikesCount)
             .Take(top)
             .ToListAsync();

            // Load KnowledgeItems into memory first
            var knowledgeItems = await _context.KnowledgeItems
                .Where(k => topItems.Select(t => t.ItemId).Contains(k.ItemId))
                .Include(k => k.Owner)
                .ToListAsync(); // <-- now it's in memory

            var leaderboard = knowledgeItems
                .Select(k => new LeaderboardDto
                {
                    ItemId = k.ItemId,
                    ItemTitle = k.Title,
                    ItemDescription = k.Description.Length > 100
                        ? k.Description.Substring(0, 100) + "..."
                        : k.Description,
                    UserId = k.OwnerId ?? Guid.Empty,
                    UserName = k.Owner != null ? k.Owner.Name : "Unknown",
                    LikesCount = topItems.FirstOrDefault(t => t.ItemId == k.ItemId)?.LikesCount ?? 0
                })
                .OrderByDescending(l => l.LikesCount)
                .ToList();

            return leaderboard;

        }

    }
}