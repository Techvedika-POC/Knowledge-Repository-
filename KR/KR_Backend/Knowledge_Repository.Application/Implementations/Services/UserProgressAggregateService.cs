using Application.Interfaces;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class UserProgressAggregateService : IUserProgressAggregateService
    {
        private readonly IModuleRepository _moduleRepo;
        private readonly ILessonRepository _lessonRepo;
        private readonly IAssessmentRepository _assessmentRepo;
        private readonly IResourceRepository _resourceRepo;
        private readonly IUserProgressRepository _progressRepo;

        public UserProgressAggregateService(
            IModuleRepository moduleRepo,
            ILessonRepository lessonRepo,
            IAssessmentRepository assessmentRepo,
            IResourceRepository resourceRepo,
            IUserProgressRepository progressRepo)
        {
            _moduleRepo = moduleRepo;
            _lessonRepo = lessonRepo;
            _assessmentRepo = assessmentRepo;
            _resourceRepo = resourceRepo;
            _progressRepo = progressRepo;
        }

        public async Task<UserPlanProgressDetailDto> GetUserPlanProgressAsync(Guid userId, Guid planId)
        {
            var modules = await _moduleRepo.GetModulesByPlanIdAsync(planId);
            var moduleDetails = new List<ModuleProgressDetailDto>();

            foreach (var module in modules)
            {
                var progress = await _progressRepo.GetModuleProgressAsync(userId, module.WeekId, module.ModuleId);
                var isUnlocked = await _progressRepo.IsModuleUnlockedAsync(userId, module.WeekId, module.ModuleId);

                var lessons = await _lessonRepo.GetByModuleIdAsync(module.ModuleId);
                var lessonDtos = lessons.Select(lesson =>
                {
                    bool isCompleted = progress?.CompletedLessonIds?.Contains(lesson.LessonId.ToString()) ?? false;

                    return new LessonProgressDto
                    {
                        LessonId = lesson.LessonId,
                        Title = lesson.Title,
                        LessonType = lesson.LessonType,
                        OrderIndex = lesson.OrderIndex ?? 0,
                        IsCompleted = isCompleted
                    };
                }).ToList();

                var assessments = await _assessmentRepo.GetByModuleIdAsync(module.ModuleId);
                var assessmentDtos = assessments.Select(a => new AssessmentProgressDto
                {
                    AssessmentId = a.AssessmentId,
                    Title = a.Title,
                    Difficulty = a.Difficulty ?? 0,
                    IsCompleted = progress?.TestStatus == "Passed",
                    IsUnlocked = isUnlocked
                }).ToList();

                var resources = await _resourceRepo.GetByTopicOrModuleAsync(Guid.Empty, module.ModuleId);
                var resourceDtos = resources.Select(r => new ResourceProgressDto
                {
                    ResourceId = r.ResourceId,
                    Title = r.Title,
                    ResourceType = r.ResourceType,
                    Url = r.Url
                }).ToList();

                moduleDetails.Add(new ModuleProgressDetailDto
                {
                    ModuleId = module.ModuleId,
                    ModuleTitle = module.ModuleName,
                    IsUnlocked = isUnlocked,
                    IsCompleted = progress?.Status == "Completed",
                    Lessons = lessonDtos,
                    Assessments = assessmentDtos,
                    Resources = resourceDtos,
                    LessonProgressPercent = progress?.LessonProgressPercent ?? 0
                });
            }

            return new UserPlanProgressDetailDto
            {
                UserId = userId,
                PlanId = planId,
                Modules = moduleDetails
            };
        }
    }
}
