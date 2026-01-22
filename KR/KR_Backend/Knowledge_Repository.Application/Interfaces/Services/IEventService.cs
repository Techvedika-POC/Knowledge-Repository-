using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IEventService
    {
        Task<List<Event>> GetAllEventsAsync();
        Task<List<Event>> GetEventsByTypeAsync(string eventType);
        Task<Event?> GetEventByIdAsync(Guid eventId);
        Task<Event> AddOrUpdateEventAsync(Event evt);
        Task<bool> DeleteEventAsync(Guid eventId);
        Task<List<EventWithTimelineDto>> GetCurrentIdeathonsAsync();
        Task<List<Event>> GetIdeathonsForMonthAsync(int year, int month);
        Task<IEnumerable<EventsByTypeDto>> GetEventsGroupedByTypeAndMonthAsync();
    }
}
