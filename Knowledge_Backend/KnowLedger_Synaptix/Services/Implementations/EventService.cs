using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class EventService : IEventService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EventService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all events with all columns of the Event table (no related entities).
        /// </summary>
        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Select(e => new Event
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    OwnerId = e.OwnerId,
                    CreatedOn = e.CreatedOn,
                    UpdatedOn = e.UpdatedOn,
                    CreatedBy = e.CreatedBy,
                    UpdatedBy = e.UpdatedBy,
                    EventType = e.EventType,
                    RegistrationCloseDate = e.RegistrationCloseDate,
                    MentorCheckpointStart = e.MentorCheckpointStart,
                    MentorCheckpointEnd = e.MentorCheckpointEnd,
                    FinalSubmissionDeadline = e.FinalSubmissionDeadline,
                    IdeaPresentationStart = e.IdeaPresentationStart,
                    IdeaPresentationEnd = e.IdeaPresentationEnd,
                    WinnersAnnouncementDate = e.WinnersAnnouncementDate,
                    ContactEmail = e.ContactEmail,
                    Notes = e.Notes
                })
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Get events filtered by type, including all Event table columns (no related entities).
        /// </summary>
        public async Task<List<Event>> GetEventsByTypeAsync(string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
                return new List<Event>();

            return await _context.Events
                .Where(e => e.EventType.ToLower() == eventType.ToLower())
                .Select(e => new Event
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    OwnerId = e.OwnerId,
                    CreatedOn = e.CreatedOn,
                    UpdatedOn = e.UpdatedOn,
                    CreatedBy = e.CreatedBy,
                    UpdatedBy = e.UpdatedBy,
                    EventType = e.EventType,
                    RegistrationCloseDate = e.RegistrationCloseDate,
                    MentorCheckpointStart = e.MentorCheckpointStart,
                    MentorCheckpointEnd = e.MentorCheckpointEnd,
                    FinalSubmissionDeadline = e.FinalSubmissionDeadline,
                    IdeaPresentationStart = e.IdeaPresentationStart,
                    IdeaPresentationEnd = e.IdeaPresentationEnd,
                    WinnersAnnouncementDate = e.WinnersAnnouncementDate,
                    ContactEmail = e.ContactEmail,
                    Notes = e.Notes
                })
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Get a single event by ID, including all Event table columns (no related entities).
        /// </summary>
        public async Task<Event?> GetEventByIdAsync(Guid eventId)
        {
            return await _context.Events
                .Where(e => e.EventId == eventId)
                .Select(e => new Event
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    OwnerId = e.OwnerId,
                    CreatedOn = e.CreatedOn,
                    UpdatedOn = e.UpdatedOn,
                    CreatedBy = e.CreatedBy,
                    UpdatedBy = e.UpdatedBy,
                    EventType = e.EventType,
                    RegistrationCloseDate = e.RegistrationCloseDate,
                    MentorCheckpointStart = e.MentorCheckpointStart,
                    MentorCheckpointEnd = e.MentorCheckpointEnd,
                    FinalSubmissionDeadline = e.FinalSubmissionDeadline,
                    IdeaPresentationStart = e.IdeaPresentationStart,
                    IdeaPresentationEnd = e.IdeaPresentationEnd,
                    WinnersAnnouncementDate = e.WinnersAnnouncementDate,
                    ContactEmail = e.ContactEmail,
                    Notes = e.Notes
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
