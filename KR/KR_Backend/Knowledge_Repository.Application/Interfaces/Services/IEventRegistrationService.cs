using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Services

{
    public interface IEventRegistrationService
    {

        Task<Team> RegisterTeamForEventAsync(EventRegistrationDto dto, Guid userId);
        Task<bool> IsUserAlreadyRegisteredAsync(Guid userId, Guid eventId);
    }
}
