using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IEventRegistrationService
    {
        /// <summary>
        /// Registers a user and their team for a specific events.
        /// </summary>
        /// <param name="dto">Team registration details (team name, members, etc.)</param>
        /// <param name="userId">Logged-in user who initiates registration</param>
        /// <returns>Registered team entity</returns>
        Task<Team> RegisterTeamForEventAsync(EventRegistrationDto dto, Guid userId);

        /// <summary>
        /// Checks if a user is already part of a team for a given event.
        /// </summary>
        Task<bool> IsUserAlreadyRegisteredAsync(Guid userId, Guid eventId);
    }
}
