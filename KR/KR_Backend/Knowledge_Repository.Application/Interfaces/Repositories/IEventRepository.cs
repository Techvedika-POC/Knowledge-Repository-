using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        
        Task<List<Event>> GetAllEventsAsync();

        Task<List<Event>> GetEventsByTypeAsync(string eventType);

        Task<Event?> GetEventByIdAsync(Guid eventId);

        Task UpdateAsync(Event evt);
        Task DeleteAsync(Event evt);
        Task<List<Event>> GetCurrentIdeathonsAsync(DateTime todayUtc);
        Task<List<Event>> GetIdeathonsForMonthAsync(int year, int month);
    }

}
