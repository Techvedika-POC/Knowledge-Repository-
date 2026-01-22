using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

public class ApproverService : IApproverService
{
    private readonly IApproverRepository _repo;

    public ApproverService(IApproverRepository repo)
    {
        _repo = repo
            ?? throw new ArgumentNullException(nameof(repo),
                "Approver repository dependency cannot be null.");
    }

    public async Task<ApproverDashboardDto> GetDashboardSummaryAsync()
    {
        try
        {
            var dto = new ApproverDashboardDto
            {
                TotalPending = await _repo.GetTotalPendingAsync(),
                NormalPending = await _repo.GetNormalPendingAsync(),
                EventPending = await _repo.GetEventPendingAsync(),
                EventWiseCounts = (await _repo.GetEventWisePendingAsync())
                    ?.Select(x => new EventPendingCountDto
                    {
                        EventId = x.EventId,
                        EventTitle = x.EventTitle,
                        PendingCount = x.Count
                    }).ToList() ?? new List<EventPendingCountDto>()
            };

            return dto;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Unable to load approver dashboard summary. Please try again later.",
                ex);
        }
    }

    public async Task<(List<KnowledgeItemDto> Items, int TotalCount)>
        GetNormalPendingAsync(int page, int size)
    {
        ValidatePaging(page, size);

        var items = await _repo.GetPendingNormalItemsAsync(page, size)
            ?? throw new InvalidOperationException(
                "Failed to retrieve pending normal items.");

        var total = await _repo.GetNormalPendingAsync();

        return (items.Select(Map).ToList(), total);
    }

    public async Task<(List<KnowledgeItemDto> Items, int TotalCount)>
        GetEventPendingAsync(int page, int size)
    {
        ValidatePaging(page, size);

        var items = await _repo.GetPendingEventItemsAsync(page, size)
            ?? throw new InvalidOperationException(
                "Failed to retrieve pending event items.");

        var total = await _repo.GetEventPendingAsync();

        return (items.Select(Map).ToList(), total);
    }

    public async Task<(List<KnowledgeItemDto> Items, int TotalCount)>
        GetPendingByEventAsync(Guid eventId, int page, int size)
    {
        ValidatePaging(page, size);

        if (eventId == Guid.Empty)
            throw new ArgumentException(
                "Event ID cannot be empty. Please provide a valid event identifier.",
                nameof(eventId));

        var items = await _repo.GetPendingItemsByEventAsync(eventId, page, size)
            ?? throw new InvalidOperationException(
                $"Failed to retrieve pending items for event '{eventId}'.");

        return (items.Select(Map).ToList(), items.Count);
    }

    public async Task<IEnumerable<string>> GetEventTypesAsync()
    {
        var types = await _repo.GetEventTypesAsync()
            ?? throw new InvalidOperationException(
                "Failed to retrieve event types.");

        return types;
    }

    public async Task<(List<KnowledgeItemDto> Items, int TotalCount)>
        GetPendingByEventTypeAsync(string eventType, int page, int size)
    {
        ValidatePaging(page, size);

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException(
                "Event type cannot be null or empty.",
                nameof(eventType));

        var items = await _repo.GetPendingItemsByEventTypeAsync(eventType, page, size)
            ?? throw new InvalidOperationException(
                $"Failed to retrieve pending items for event type '{eventType}'.");

        var total = await _repo.GetPendingEventTypeCountAsync(eventType);

        return (items.Select(Map).ToList(), total);
    }

    public async Task<bool> ApproveAsync(Guid itemId, Guid approverId)
    {
        ValidateIds(itemId, approverId);

        var result = await _repo.ApproveAsync(itemId, approverId);

        if (!result)
            throw new InvalidOperationException(
                $"Approval failed. The item '{itemId}' may not exist or has already been processed.");

        return true;
    }

    public async Task<bool> RejectAsync(Guid itemId, Guid approverId, string feedback)
    {
        ValidateIds(itemId, approverId);

        if (string.IsNullOrWhiteSpace(feedback))
            throw new ArgumentException("Feedback is required when rejecting.");

        var success = await _repo.RejectAsync(itemId, approverId, feedback);

        if (!success)
            throw new KeyNotFoundException(
                $"Rejection failed. The item '{itemId}' does not exist or was already processed.");

        return true;
    }


    private static void ValidatePaging(int page, int size)
    {
        if (page <= 0)
            throw new ArgumentException(
                "Page number must be greater than zero.",
                nameof(page));

        if (size <= 0)
            throw new ArgumentException(
                "Page size must be greater than zero.",
                nameof(size));
    }

    private static void ValidateIds(Guid itemId, Guid approverId)
    {
        if (itemId == Guid.Empty)
            throw new ArgumentException(
                "Item ID cannot be empty.",
                nameof(itemId));

        if (approverId == Guid.Empty)
            throw new ArgumentException(
                "Approver ID cannot be empty.",
                nameof(approverId));
    }

    private static KnowledgeItemDto Map(KnowledgeItem k)
    {
        return new KnowledgeItemDto
        {
            ItemId = k.ItemId,
            Title = k.Title,
            Description = k.Description,
            DomainName = k.Domain?.DomainName,
            CategoryName = k.Category?.CategoryName,
            CreatedByName = k.Owner?.Name ?? "Unknown",
            Framework = k.Framework,
            Language = k.Language,
            IsEventItem = k.IsEventItem
        };
    }
}
