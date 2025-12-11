using Application.Interfaces;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class UserProgressService : IUserProgressService
    {
        private readonly IModuleRepository _moduleRepo;
        private readonly IUserProgressRepository _progressRepo;

        public UserProgressService(IModuleRepository moduleRepo, IUserProgressRepository progressRepo)
        {
            _moduleRepo = moduleRepo;
            _progressRepo = progressRepo;
        }

        // -----------------------------------------------------------
        // GET USER PROGRESS ACROSS MODULES
        // -----------------------------------------------------------
        public async Task<UserProgressDto> GetUserProgressAsync(Guid userId, Guid planId)
        {
            var modules = await _moduleRepo.GetModulesByPlanIdAsync(planId);
            var moduleStatuses = new List<ModuleStatusDto>();

            foreach (var module in modules)
            {
                var progress = await _progressRepo.GetModuleProgressAsync(userId, module.WeekId, module.ModuleId);
                var unlocked = await _progressRepo.IsModuleUnlockedAsync(userId, module.WeekId, module.ModuleId);

                moduleStatuses.Add(new ModuleStatusDto
                {
                    ModuleId = module.ModuleId,
                    ModuleTitle = module.ModuleName,
                    IsUnlocked = unlocked,
                    IsCompleted = progress?.Status == "Completed",
                    TestStatus = progress?.TestStatus ?? "NotTaken",
                    LessonProgressPercent = progress?.LessonProgressPercent ?? 0
                });
            }

            return new UserProgressDto
            {
                UserId = userId,
                PlanId = planId,
                Modules = moduleStatuses
            };
        }

        // -----------------------------------------------------------
        // MARK MODULE COMPLETED (ONLY IF RULES MET)
        // -----------------------------------------------------------
        public async Task<bool> TryMarkModuleCompletedAsync(Guid userId, Guid weekId, Guid moduleId)
        {
            return await _progressRepo.TryMarkModuleCompletedAsync(userId, weekId, moduleId);
        }

        // -----------------------------------------------------------
        // TEST STATUS UPDATE (Pass / Fail)
        // -----------------------------------------------------------
        public async Task UpdateTestStatusAsync(Guid userId, Guid weekId, Guid moduleId, string testStatus)
        {
            await _progressRepo.UpdateTestStatusAsync(userId, weekId, moduleId, testStatus);
        }

        // -----------------------------------------------------------
        // LESSON ACCESS TRACKING
        // -----------------------------------------------------------
        public async Task TrackLessonAccessAsync(Guid userId, Guid moduleId, Guid lessonId)
        {
            await _progressRepo.TrackLessonAccessAsync(userId, moduleId, lessonId);
        }

        // -----------------------------------------------------------
        // LESSON COMPLETION
        // -----------------------------------------------------------
        public async Task MarkLessonCompletedAsync(Guid userId, Guid moduleId, Guid lessonId)
        {
            await _progressRepo.MarkLessonCompletedAsync(userId, moduleId, lessonId);
        }
    }
}
