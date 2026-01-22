// File: Knowledge_Repository.Application.Interfaces.Repositories/IMentorRepository.cs
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IMentorRepository
    {
      
        Task<IEnumerable<Team>> GetAssignedTeamsAsync(Guid mentorId);
        Task<Team> GetTeamDetailsAsync(Guid teamId);
        Task<string> GetMentorNameAsync(Guid mentorId);
        Task<string> GetUserNameAsync(Guid userId);
        Task<Mentor?> GetByUserIdAsync(Guid userId);
        Task<Mentor?> GetByUserAndEventAsync(Guid userId, Guid eventId);
        Task<Mentor?> GetByIdAsync(Guid mentorId);
        Task<Mentor?> GetByUserAndTeamAsync(Guid userId, Guid teamId);
        Task<IEnumerable<Mentor>> GetByUserAsync(Guid userId);

    }
}
