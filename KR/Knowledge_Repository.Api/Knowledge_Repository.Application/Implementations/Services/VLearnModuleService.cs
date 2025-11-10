using KnowLedger_Synaptix.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class VLearnModuleService : IVLearnModuleService
    {
        private readonly IVLearnModuleRepository _moduleRepository;

        public VLearnModuleService(IVLearnModuleRepository moduleRepository)
        {
            _moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(moduleRepository));
        }

        /// <summary>
        /// Get modules under a topic for a specific user and apply lock/unlock logic.
        /// </summary>
        public async Task<IEnumerable<VLearnModuleDto>> GetModulesByTopicAndUserAsync(Guid topicId, Guid userId)
        {
            var modules = await _moduleRepository.GetModulesByTopicAndUserAsync(topicId, userId);

            var moduleDtos = modules.Select(module =>
            {
                var userProgress = module.UserModuleProgresses?.FirstOrDefault(p => p.UserId == userId);

                return new VLearnModuleDto
                {
                    ModuleId = module.ModuleId,
                    ModuleName = module.ModuleName,
                    Description = module.Description,
                    ContentLink = module.ContentLink,
                    OrderNo = module.OrderNo,
                    Status = userProgress?.Status ?? "Not Started",
                    TestStatus = userProgress?.TestStatus ?? "Not Started",
                    IsLocked = true 
                };
            })
            .OrderBy(m => m.OrderNo)
            .ToList();

            bool unlockNext = true;
            foreach (var mod in moduleDtos)
            {
                if (unlockNext)
                    mod.IsLocked = false;

                if (mod.Status != "Completed" || mod.TestStatus != "Passed")
                    unlockNext = false;
            }

            return moduleDtos;
        }

        /// <summary>
        /// Update test result for a module for a specific user.
        /// </summary>
        public async Task<bool> UpdateTestStatusAsync(VLearnTestResultDto result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            return await _moduleRepository.UpdateTestStatusAsync(
                result.ModuleId,
                result.UserId,
                result.TestStatus == "Passed"
            );
        }
    }
}
