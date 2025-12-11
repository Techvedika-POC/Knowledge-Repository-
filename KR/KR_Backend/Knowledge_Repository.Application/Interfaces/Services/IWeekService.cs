using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IWeekService
    {
        Task<WeekDto> CreateWeekAsync(Guid planId, WeekDto weekDto);
        Task<IEnumerable<WeekDto>> GetWeeksByPlanAsync(Guid planId);
        Task<WeekDto?> GetWeekByIdAsync(Guid weekId);
        Task<WeekProgressDto?> GetWeekProgressAsync(Guid weekId, Guid userId);
        Task UpdateWeekAsync(Guid weekId, WeekDto weekDto);
        Task DeleteWeekAsync(Guid weekId);

        // Full DTO methods
        Task<WeekFullDto?> GetWeekFullByIdAsync(Guid weekId, Guid? userId = null);
        Task<IEnumerable<WeekFullDto>> GetWeeksFullByPlanAsync(Guid planId, Guid? userId = null);
    }
}
