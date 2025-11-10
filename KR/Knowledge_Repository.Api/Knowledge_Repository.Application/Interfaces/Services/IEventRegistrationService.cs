using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Services

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
