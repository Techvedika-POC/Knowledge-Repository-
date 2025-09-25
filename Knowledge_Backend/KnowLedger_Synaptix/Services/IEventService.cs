using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services
{
    public interface IEventService
    {
        Task<List<EventDto>> GetAllEventsAsync();
    }
}
