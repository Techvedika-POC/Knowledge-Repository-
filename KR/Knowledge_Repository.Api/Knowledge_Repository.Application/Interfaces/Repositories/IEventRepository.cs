using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        /// <summary>
        /// Retrieves all events.
        /// </summary>
        Task<List<Event>> GetAllEventsAsync();

        /// <summary>
        /// Retrieves events filtered by a specific type (e.g., Hackathon, Workshop).
        /// </summary>
        Task<List<Event>> GetEventsByTypeAsync(string eventType);

        /// <summary>
        /// Retrieves a specific event by ID.
        /// </summary>
        Task<Event?> GetEventByIdAsync(Guid eventId);

        /// <summary>
        /// Updates an existing event.
        /// </summary>
        Task UpdateAsync(Event evt);

        /// <summary>
        /// Deletes an event from the system.
        /// </summary>
        Task DeleteAsync(Event evt);
    }
}
