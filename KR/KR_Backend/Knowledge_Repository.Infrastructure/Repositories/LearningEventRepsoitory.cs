using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;

namespace Knowledge_Repository.Infrastructure.Repositories;
public class LearningEventRepository : ILearningEventRepository
{
    private readonly Knowledge_Repository_dbContext _context;

    public LearningEventRepository(Knowledge_Repository_dbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        Guid userId,
        string eventType,
        string entityType,
        Guid? entityId,
        string? metadata = null)
    {
        var ev = new LearningEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            Metadata = metadata,
            CreatedOn = DateTime.UtcNow
        };

        _context.LearningEvents.Add(ev);
        await _context.SaveChangesAsync();
    }
}


