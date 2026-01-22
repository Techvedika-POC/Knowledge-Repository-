using Knowledge_Repository.Application.Dtos;
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
        public async Task<List<EventWithTimelineDto>> GetCurrentIdeathonsAsync()
        {
            var todayUtc = DateTime.UtcNow;

            var events = await _eventRepository.GetCurrentIdeathonsAsync(todayUtc);

            return events.Select(e => new EventWithTimelineDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                RegistrationCloseDate = e.RegistrationCloseDate,
                MentorCheckpointStart = e.MentorCheckpointStart,
                MentorCheckpointEnd = e.MentorCheckpointEnd,
                FinalSubmissionDeadline = e.FinalSubmissionDeadline,
                IdeaPresentationStart = e.IdeaPresentationStart,
                IdeaPresentationEnd = e.IdeaPresentationEnd,
                WinnersAnnouncementDate = e.WinnersAnnouncementDate,
                ContactEmail = e.ContactEmail,
                Notes = e.Notes,
                EventType = e.EventType,
                CreatedOn = e.CreatedOn,
                UpdatedOn = e.UpdatedOn
            }).ToList();
        }

        public async Task<List<Event>> GetIdeathonsForMonthAsync(int year, int month)
        {
            return await _eventRepository.GetIdeathonsForMonthAsync(year, month);
        }
        public async Task<IEnumerable<EventsByTypeDto>> GetEventsGroupedByTypeAndMonthAsync()
        {
            var allEvents = await _eventRepository.GetAllEventsAsync();
            if (allEvents == null || allEvents.Count == 0)
                return Enumerable.Empty<EventsByTypeDto>();

            var mapped = allEvents
                .Where(e => e != null)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Title = e.Title ?? string.Empty,
                    Description = e.Description ?? string.Empty,
                    EventType = string.IsNullOrWhiteSpace(e.EventType) ? "Unknown" : e.EventType.Trim(),
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    OwnerId = e.OwnerId,
                    CreatedOn = e.CreatedOn,
                    UpdatedOn = e.UpdatedOn,
                    RegistrationCloseDate = e.RegistrationCloseDate,
                    MentorCheckpointStart = e.MentorCheckpointStart,
                    MentorCheckpointEnd = e.MentorCheckpointEnd,
                    FinalSubmissionDeadline = e.FinalSubmissionDeadline,
                    IdeaPresentationStart = e.IdeaPresentationStart,
                    IdeaPresentationEnd = e.IdeaPresentationEnd,
                    WinnersAnnouncementDate = e.WinnersAnnouncementDate,
                    ContactEmail = e.ContactEmail ?? string.Empty,
                    Notes = e.Notes ?? string.Empty
                })
                .ToList();

            var result = new List<EventsByTypeDto>();


            var groupedByType = mapped
                .GroupBy(x => x.EventType)
                .OrderBy(g => g.Key);

            foreach (var typeGroup in groupedByType)
            {

                var monthBuckets = new Dictionary<(int Year, int Month), EventsByMonthDto>();

                foreach (var ev in typeGroup)
                {
                    DateOnly? groupDate = ev.StartDate ?? ev.EndDate;

                    int year, month;
                    if (groupDate.HasValue)
                    {
                        year = groupDate.Value.Year;
                        month = groupDate.Value.Month;
                    }
                    else
                    {
                        year = 1; month = 1;
                    }

                    var key = (Year: year, Month: month);

                    if (!monthBuckets.TryGetValue(key, out var monthDto))
                    {
                        var label = (year == 1 && month == 1)
                            ? "Undated"
                            : new DateTime(year, month, 1).ToString("MMMM yyyy");

                        monthDto = new EventsByMonthDto
                        {
                            Year = year,
                            Month = month,
                            MonthLabel = label,
                            Events = new List<EventDto>()
                        };

                        monthBuckets[key] = monthDto;
                    }

                    monthDto.Events.Add(ev);
                }

                var monthsList = monthBuckets
                    .Values
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => m.Month)
                    .ToList();

                var dated = monthsList.Where(m => m.Year != 1).ToList();
                var undated = monthsList.Where(m => m.Year == 1).ToList();
                var finalMonths = dated.Concat(undated).ToList();
                foreach (var m in finalMonths)
                    m.Events = m.Events.OrderBy(e => e.Title).ToList();

                result.Add(new EventsByTypeDto
                {
                    EventType = typeGroup.Key,
                    Months = finalMonths
                });
            }

            return result;
        }


    }
}