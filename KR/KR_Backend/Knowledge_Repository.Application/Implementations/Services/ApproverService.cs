using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

public class ApproverService : IApproverService
{
    private readonly IApproverRepository _repo;

    public ApproverService(IApproverRepository repo)
    {
        _repo = repo;
    }

    public async Task<ApproverDashboardDto> GetDashboardSummaryAsync()
    {
        var dto = new ApproverDashboardDto
        {
            TotalPending = await _repo.GetTotalPendingAsync(),
            NormalPending = await _repo.GetNormalPendingAsync(),
            EventPending = await _repo.GetEventPendingAsync(),
            EventWiseCounts = (await _repo.GetEventWisePendingAsync())
                .Select(x => new EventPendingCountDto
                {
                    EventId = x.EventId,
                    EventTitle = x.EventTitle,
                    PendingCount = x.Count
                }).ToList()
        };
        return dto;
    }

    public async Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetNormalPendingAsync(int page, int size)
    {
        var items = await _repo.GetPendingNormalItemsAsync(page, size);
        return (items.Select(Map).ToList(), await _repo.GetNormalPendingAsync());
    }

    public async Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetEventPendingAsync(int page, int size)
    {
        var items = await _repo.GetPendingEventItemsAsync(page, size);
        return (items.Select(Map).ToList(), await _repo.GetEventPendingAsync());
    }

    public async Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetPendingByEventAsync(Guid eventId, int page, int size)
    {
        var items = await _repo.GetPendingItemsByEventAsync(eventId, page, size);
        return (items.Select(Map).ToList(), items.Count);
    }
    public async Task<IEnumerable<string>> GetEventTypesAsync()
    {
        return await _repo.GetEventTypesAsync();
    }
    public async Task<(List<KnowledgeItemDto> Items, int TotalCount)>
    GetPendingByEventTypeAsync(string eventType, int page, int size)
    {
        var items = await _repo.GetPendingItemsByEventTypeAsync(eventType, page, size);
        var total = await _repo.GetPendingEventTypeCountAsync(eventType);

        return (items.Select(Map).ToList(), total);
    }



    private KnowledgeItemDto Map(KnowledgeItem k)
    {
        return new KnowledgeItemDto
        {
            ItemId = k.ItemId,
            Title = k.Title,
            Description = k.Description,
            DomainName = k.Domain?.DomainName,
            CategoryName = k.Category?.CategoryName,
            CreatedByName = k.Owner?.Name,
            Framework = k.Framework,
            Language = k.Language,
            IsEventItem = k.IsEventItem
        };
    }

    public Task<bool> ApproveAsync(Guid itemId, Guid approverId) =>
        _repo.ApproveAsync(itemId, approverId);

    public Task<bool> RejectAsync(Guid itemId, Guid approverId) =>
        _repo.RejectAsync(itemId, approverId);
}
