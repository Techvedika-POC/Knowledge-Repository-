using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IWeekRepository : IGenericRepository<Week>
    {
        Task<IEnumerable<Week>> GetByPlanIdAsync(Guid planId);
        Task<Week?> GetWeekWithModulesAsync(Guid weekId);
        Task<bool> IsWeekUnlockedAsync(Guid weekId, Guid userId);

        // New methods for full DTOs
        Task<Week?> GetWeekFullByIdAsync(Guid weekId);
        Task<IEnumerable<Week>> GetWeeksFullByPlanAsync(Guid planId);
    }
}
