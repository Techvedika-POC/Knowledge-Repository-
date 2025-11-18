using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _eventRepository.GetAllEventsAsync();
        }

        public async Task<List<Event>> GetEventsByTypeAsync(string eventType)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                return new List<Event>();

            return await _eventRepository.GetEventsByTypeAsync(eventType);
        }

        public async Task<Event?> GetEventByIdAsync(Guid eventId)
        {
            if (eventId == Guid.Empty)
                return null;

            return await _eventRepository.GetEventByIdAsync(eventId);
        }

        public async Task<Event> AddOrUpdateEventAsync(Event evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            if (string.IsNullOrWhiteSpace(evt.Title))
                throw new ArgumentException("Event title is required.");
            if (evt.StartDate == null || evt.EndDate == null)
                throw new ArgumentException("StartDate and EndDate must be provided.");
            if (evt.EndDate < evt.StartDate)
                throw new ArgumentException("EndDate cannot be earlier than StartDate.");

            var existing = evt.EventId != Guid.Empty
                ? await _eventRepository.GetEventByIdAsync(evt.EventId)
                : null;

            if (existing != null)
            {
                existing.Title = evt.Title;
                existing.Description = evt.Description;
                existing.EventType = evt.EventType;
                existing.StartDate = evt.StartDate;
                existing.EndDate = evt.EndDate;
                existing.RegistrationCloseDate = evt.RegistrationCloseDate;
                existing.MentorCheckpointStart = evt.MentorCheckpointStart;
                existing.MentorCheckpointEnd = evt.MentorCheckpointEnd;
                existing.FinalSubmissionDeadline = evt.FinalSubmissionDeadline;
                existing.IdeaPresentationStart = evt.IdeaPresentationStart;
                existing.IdeaPresentationEnd = evt.IdeaPresentationEnd;
                existing.WinnersAnnouncementDate = evt.WinnersAnnouncementDate;
                existing.ContactEmail = evt.ContactEmail;
                existing.Notes = evt.Notes;
                existing.UpdatedOn = DateTime.UtcNow;

                await _eventRepository.UpdateAsync(existing);
                return existing;
            }
            else
            {
                evt.EventId = Guid.NewGuid();
                evt.CreatedOn = DateTime.UtcNow;
                await _eventRepository.AddAsync(evt);
                return evt;
            }
        }

        public async Task<bool> DeleteEventAsync(Guid eventId)
        {
            var existing = await _eventRepository.GetEventByIdAsync(eventId);
            if (existing == null)
                return false;

            await _eventRepository.DeleteAsync(existing);
            return true;
        }


        // -----------------------------------------------------------
        // NEW SERVICE METHODS
        // -----------------------------------------------------------

        /// <summary>
        /// Get all events belonging to the current month
        /// </summary>
        public async Task<List<Event>> GetCurrentMonthEventsAsync()
        {
            return await _eventRepository.GetCurrentMonthEventsAsync();
        }

        /// <summary>
        /// Get events that are actively running today
        /// </summary>
        public async Task<List<Event>> GetActiveEventsAsync()
        {
            return await _eventRepository.GetActiveEventsAsync();
        }
    }
}
