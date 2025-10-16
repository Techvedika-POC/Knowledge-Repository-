using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
public class EngagementService : IEngagementService
{
    private readonly Knowledge_Repository_dbContext _context;

    public EngagementService(Knowledge_Repository_dbContext context)
    {
        _context = context;
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

        _context.Engagements.Add(engagement);
        await _context.SaveChangesAsync();
    }

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

    public async Task<KnowledgeItemEngagementDto> GetEngagementSummaryAsync(Guid itemId, Guid userId)
    {
        var likesCount = await _context.Engagements
            .CountAsync(e => e.ItemId == itemId && e.EngagementType == "Like");

        var favouritesCount = await _context.Engagements
            .CountAsync(e => e.ItemId == itemId && e.EngagementType == "Favourite");

        var comments = (await _context.Engagements
            .Where(e => e.ItemId == itemId && e.EngagementType == "Comment")
            .Include(e => e.User)
            .ToListAsync())
            .Select(e => new CommentDto
            {
                EngagementId = e.EngagementId,
                UserId = (Guid)e.UserId,
                UserName = e.User?.Name ?? "Unknown",
                CommentText = e.CommentText ?? "",
                CreatedOn = (DateTime)e.CreatedOn
            })
            .ToList();

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


    public async Task<int> GetLikesCountAsync(Guid itemId)
    {
        return await _context.Engagements
            .CountAsync(e => e.ItemId == itemId && e.EngagementType == "Like");
    }
    public async Task<List<UserEngagementDto>> GetUserEngagementsAsync(Guid userId)
    {
        return await _context.Engagements
            .Where(e => e.UserId == userId && (e.EngagementType == "Like" || e.EngagementType == "Favourite"))
            .Select(e => new UserEngagementDto
            {
                ItemId = (Guid)e.ItemId,
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
