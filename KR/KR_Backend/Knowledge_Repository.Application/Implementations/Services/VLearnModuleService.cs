using KnowLedger_Synaptix.Dtos;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class VLearnModuleService : IVLearnModuleService
    {
        private readonly IVLearnModuleRepository _moduleRepo;
        private readonly IVLearnTopicRepository _topicRepo;

        public VLearnModuleService(IVLearnModuleRepository moduleRepo, IVLearnTopicRepository topicRepo)
        {
            _moduleRepo = moduleRepo ?? throw new ArgumentNullException(nameof(moduleRepo));
            _topicRepo = topicRepo ?? throw new ArgumentNullException(nameof(topicRepo));
        }

        public async Task<IEnumerable<VLearnModuleDto>> GetModulesByTopicAndUserAsync(Guid topicId, Guid userId)
        {
            // load modules + user progress via repository Query() if available, otherwise use repo method
            var modulesQuery = _moduleRepo.Query().Where(m => m.TopicId == topicId);

            var modules = await modulesQuery
                .Include(m => m.UserModuleProgresses.Where(ump => ump.UserId == userId))
                .OrderBy(m => m.OrderNo)
                .ToListAsync();

            var dtos = modules.Select(m =>
            {
                var userProgress = m.UserModuleProgresses?.FirstOrDefault(p => p.UserId == userId);
                return new VLearnModuleDto
                {
                    ModuleId = m.ModuleId,
                    ModuleName = m.ModuleName,
                    Description = m.Description,
                    ContentLink = m.ContentLink,
                    OrderNo = m.OrderNo,
                    Status = userProgress?.Status ?? "Not Started",
                    TestStatus = userProgress?.TestStatus ?? "Not Started",
                    IsLocked = true
                };
            })
            .OrderBy(x => x.OrderNo)
            .ToList();

            bool unlockNext = true;
            foreach (var dto in dtos)
            {
                if (unlockNext) dto.IsLocked = false;
                if (dto.Status != "Completed" || dto.TestStatus != "Passed")
                    unlockNext = false;
            }

            return dtos;
        }

        public async Task<IEnumerable<VLearnModuleDto>> GetModulesByTopicAsync(Guid topicId)
        {
            var modules = await _moduleRepo.GetModulesByTopicAsync(topicId);
            var dtos = modules
                .OrderBy(m => m.OrderNo)
                .Select(m => new VLearnModuleDto
                {
                    ModuleId = m.ModuleId,
                    ModuleName = m.ModuleName,
                    Description = m.Description,
                    ContentLink = m.ContentLink,
                    OrderNo = m.OrderNo,
                    Status = "Not Started",
                    TestStatus = "Not Started",
                    IsLocked = true
                })
                .ToList();

            if (dtos.Any())
            {
                dtos = dtos.OrderBy(d => d.OrderNo).ToList();
                dtos.First().IsLocked = false;
            }

            return dtos;
        }

        public async Task<VLearnModuleDto> AddModuleAsync(Guid topicId, CreateModuleDto dto, Guid createdBy)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.ModuleName)) throw new ArgumentException("ModuleName is required", nameof(dto.ModuleName));

            var topic = await _topicRepo.GetByIdAsync(topicId);
            if (topic == null) throw new KeyNotFoundException("Topic not found");

            var exists = await _moduleRepo.ModuleNameExistsInTopicAsync(topicId, dto.ModuleName);
            if (exists) throw new InvalidOperationException("Module with same name already exists in this topic.");

            var module = new Module
            {
                ModuleId = Guid.NewGuid(),
                TopicId = topicId,
                ModuleName = dto.ModuleName.Trim(),
                Description = dto.Description,
                ContentLink = dto.ContentLink,
                OrderNo = dto.OrderNo,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy == Guid.Empty ? (Guid?)null : createdBy
            };

            await _moduleRepo.AddModuleAsync(module);

            var result = new VLearnModuleDto
            {
                ModuleId = module.ModuleId,
                ModuleName = module.ModuleName,
                Description = module.Description,
                ContentLink = module.ContentLink,
                OrderNo = module.OrderNo,
                Status = "Not Started",
                TestStatus = "Not Started",
                IsLocked = true
            };

            return result;
        }

        public async Task<bool> UpdateTestStatusAsync(VLearnTestResultDto result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return await _moduleRepo.UpdateTestStatusAsync(result.ModuleId, result.UserId, result.TestStatus == "Passed");
        }
    }
}
