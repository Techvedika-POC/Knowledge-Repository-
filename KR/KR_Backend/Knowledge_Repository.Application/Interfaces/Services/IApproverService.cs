using Knowledge_Repository.Application.Dtos;

public interface IApproverService
{
    Task<ApproverDashboardDto> GetDashboardSummaryAsync();

    Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetNormalPendingAsync(int page, int size);
    Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetEventPendingAsync(int page, int size);
    Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetPendingByEventAsync(Guid eventId, int page, int size);
    Task<IEnumerable<string>> GetEventTypesAsync();

    Task<bool> ApproveAsync(Guid itemId, Guid approverId);
    Task<bool> RejectAsync(Guid itemId, Guid approverId, string feedback);

    Task<(List<KnowledgeItemDto> Items, int TotalCount)>
    GetPendingByEventTypeAsync(string eventType, int page, int size);

}
