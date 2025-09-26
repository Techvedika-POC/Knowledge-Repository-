using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IEventService
    {
        Task<List<EventDto>> GetAllEventsAsync();
    }
}
