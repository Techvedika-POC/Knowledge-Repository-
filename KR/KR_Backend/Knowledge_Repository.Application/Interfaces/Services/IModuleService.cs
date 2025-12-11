// File: Knowledge_Repository.Application.Interfaces.Services/IModuleService.cs
using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IModuleService
    {
        Task<ModuleDto> CreateModuleAsync(Guid weekId, ModuleDto moduleDto);
        Task<IEnumerable<ModuleDto>> GetModulesByWeekAsync(Guid weekId);
        Task<ModuleDetailDto?> GetModuleDetailAsync(Guid moduleId, Guid userId);
        Task<ModuleProgressDto?> GetModuleProgressAsync(Guid moduleId, Guid userId);
        Task UpdateModuleAsync(Guid moduleId, ModuleDto moduleDto);
        Task DeleteModuleAsync(Guid moduleId);
    }
}
