using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IEventService
    {

        /// <summary>
        /// Retrieves all events available in the system.
        /// </summary>
        Task<List<Event>> GetAllEventsAsync();

        /// <summary>
        /// Retrieves events filtered by a specific event type.
        /// </summary>
        Task<List<Event>> GetEventsByTypeAsync(string eventType);
    }
}
