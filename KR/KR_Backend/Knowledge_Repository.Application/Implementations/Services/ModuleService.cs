using Application.Interfaces;
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
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepo;
        private readonly IUserModuleProgressRepository _moduleProgressRepo;
        private readonly ILessonRepository _lessonRepo;
        private readonly IUserLessonProgressRepository _lessonProgressRepo;
        private readonly IUserAssessmentProgressRepository _assessmentProgressRepo;
        private readonly IAssessmentRepository _assessmentRepo;


        public ModuleService(
        IModuleRepository moduleRepo,
        IUserModuleProgressRepository moduleProgressRepo,
        ILessonRepository lessonRepo,
        IUserLessonProgressRepository lessonProgressRepo,
          IUserAssessmentProgressRepository assessmentProgressRepo,
           IAssessmentRepository assessmentRepo)
        {
            _moduleRepo = moduleRepo;
            _moduleProgressRepo = moduleProgressRepo;
            _lessonRepo = lessonRepo;
            _lessonProgressRepo = lessonProgressRepo;
            _assessmentProgressRepo = assessmentProgressRepo;
            _assessmentRepo = assessmentRepo;
        }
        public async Task<ModuleDto> CreateModuleAsync(Guid weekId, ModuleDto dto)
        {
            var module = new Module
            {
                ModuleId = Guid.NewGuid(),
                WeekId = weekId,
                ModuleName = dto.ModuleName,
                Description = dto.Description,
                Overview = dto.Overview,
                Description1 = dto.ExtendedDescription,
                Prerequisites = dto.Prerequisites,
                DurationDays = dto.DurationDays,
                OrderNo = dto.OrderNo,
                IsAiGenerated = dto.IsAiGenerated,
                Metadata = dto.Metadata,
                CreatedOn = DateTime.UtcNow
            };

            await _moduleRepo.AddAsync(module);

            dto.ModuleId = module.ModuleId;
            return dto;
        }
        public async Task<IEnumerable<ModuleDto>> GetModulesByWeekAsync(
         Guid weekId,
         Guid userId)
        {
            var modules = (await _moduleRepo.GetByWeekIdAsync(weekId))
                .OrderBy(m => m.OrderNo)
                .ToList();

            var result = new List<ModuleDto>();

            bool previousCompleted = true; 

            foreach (var m in modules)
            {
                var moduleProgress = await _moduleProgressRepo
                    .GetAsync(userId, m.ModuleId);

                bool isCompleted =
                    moduleProgress != null &&
                    moduleProgress.Status == "Completed";

                bool isUnlocked = previousCompleted;

                var lessons = (await _lessonRepo
                    .GetByModuleIdAsync(m.ModuleId))
                    .ToList();

                var lessonProgress =
                    await _lessonProgressRepo
                        .GetByModuleAsync(userId, m.ModuleId);

                int totalLessons = lessons.Count;

                int completedLessons =
                    lessonProgress.Count(lp =>
                        lp.Status == "Completed");

                int progressPercent =
                    totalLessons == 0
                        ? 0
                        : (completedLessons * 100) / totalLessons;

                result.Add(new ModuleDto
                {
                    ModuleId = m.ModuleId,
                    WeekId = m.WeekId,
                    ModuleName = m.ModuleName ?? "",
                    Description = m.Description ?? "",
                    Overview = m.Overview ?? "",
                    ExtendedDescription = m.Description1 ?? "",
                    Prerequisites = m.Prerequisites ?? "",
                    DurationDays = m.DurationDays,
                    OrderNo = m.OrderNo ?? 0,
                    IsAiGenerated = m.IsAiGenerated ?? false,
                    Metadata = m.Metadata ?? "",

                    IsUnlocked = isUnlocked,
                    IsCompleted = isCompleted,
                    LessonProgressPercent = progressPercent
                });

                previousCompleted = isCompleted;
            }

            return result;
        }
        public async Task<ModuleDetailDto?> GetModuleDetailAsync(
            Guid moduleId,
            Guid userId)
        {
            var module = await _moduleRepo.GetModuleFullAsync(moduleId);
            if (module == null) return null;
            var lessonProgress =
                await _lessonProgressRepo.GetByModuleAsync(userId, moduleId);

            var assessmentProgress =
                await _assessmentProgressRepo.GetByModuleAsync(userId, moduleId);

            return new ModuleDetailDto
            {
                ModuleId = module.ModuleId,
                WeekId = module.WeekId,
                ModuleName = module.ModuleName ?? "",
                Description = module.Description ?? "",
                Overview = module.Overview ?? "",
                ExtendedDescription = module.Description1 ?? "",
                Prerequisites = module.Prerequisites ?? "",
                DurationDays = module.DurationDays,
                OrderNo = module.OrderNo ?? 0,
                IsAiGenerated = module.IsAiGenerated ?? false,
                Metadata = module.Metadata ?? "",
                Lessons = module.Lessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l =>
                    {
                        var lp = lessonProgress
                            .FirstOrDefault(x => x.LessonId == l.LessonId);

                        return new LessonDto
                        {
                            LessonId = l.LessonId,
                            ModuleId = l.ModuleId,
                            Title = l.Title ?? "",
                            Content = l.Content ?? "",
                            LessonType = l.LessonType ?? "Text",
                            OrderIndex = l.OrderIndex ?? 0,
                            Overview = l.Overview ?? "",
                            Prerequisites = l.Prerequisites ?? "",
                            DurationMinutes = l.DurationMinutes,
                            IsCompleted = lp?.Status == "Completed"
                        };
                    })
                    .ToList(),

                Resources = module.Resources
                    .Select(r => new ResourceDto
                    {
                        ResourceId = r.ResourceId,
                        ModuleId = r.ModuleId,
                        TopicId = r.TopicId,
                        Title = r.Title ?? "",
                        Url = r.Url ?? "",
                        ResourceType = r.ResourceType ?? "Link",
                        Description = r.Description ?? "",
                        Metadata = r.Metadata ?? "",
                        IsAiGenerated = r.IsAiGenerated ?? false
                    })
                    .ToList(),

                Assessments = module.Assessments
    .OrderBy(a => a.CreatedOn)
    .Select(a =>
    {
        var latestResult =
            _assessmentRepo.GetLatestResultAsync(userId, a.AssessmentId)
                .GetAwaiter()
                .GetResult();

        return new AssessmentDto
        {
            AssessmentId = a.AssessmentId,
            ModuleId = a.ModuleId,
            TopicId = a.TopicId,
            Title = a.Title ?? "",
            Difficulty = a.Difficulty ?? 1,
            Description = a.Description ?? "",
            LearningObjectives = a.LearningObjectives ?? "",
            Metadata = a.Metadata ?? "",
            IsAiGenerated = a.IsAiGenerated ?? false,

            Questions = a.AssessmentQuestions
                .Select(q => new AssessmentQuestionDto
                {
                    QuestionId = q.QuestionId,
                    Question = q.Question ?? "",
                    Options = q.Options ?? "",
                    CorrectAnswer = q.CorrectAnswer ?? "",
                    Explanation = q.Explanation ?? "",
                    Hint = q.Hint ?? "",
                    QuestionType = q.QuestionType ?? "multiple-choice",
                    Metadata = q.Metadata ?? ""
                })
                .ToList(),

            LatestResult = latestResult   
        };
    })
    .ToList()


            };
        }


        public async Task UpdateModuleAsync(Guid moduleId, ModuleDto dto)
        {
            var module = await _moduleRepo.GetByIdAsync(moduleId);
            if (module == null) throw new Exception("Module not found");

            module.ModuleName = dto.ModuleName;
            module.Description = dto.Description;
            module.Overview = dto.Overview;
            module.Description1 = dto.ExtendedDescription;
            module.Prerequisites = dto.Prerequisites;
            module.DurationDays = dto.DurationDays;
            module.OrderNo = dto.OrderNo;
            module.IsAiGenerated = dto.IsAiGenerated;
            module.Metadata = dto.Metadata;
            module.UpdatedOn = DateTime.UtcNow;

            await _moduleRepo.UpdateAsync(module);
        }

        public async Task DeleteModuleAsync(Guid moduleId)
        {
            var module = await _moduleRepo.GetByIdAsync(moduleId);
            if (module == null) throw new Exception("Module not found");

            await _moduleRepo.DeleteAsync(module);
        }

        public async Task<ModuleProgressDto?> GetModuleProgressAsync(
      Guid moduleId,
      Guid userId)
        {
            var module = await _moduleRepo.GetByIdAsync(moduleId);
            if (module == null) return null;
            var weekModules = await _moduleRepo
                .GetByWeekIdAsync(module.WeekId);

            var ordered = weekModules
                .OrderBy(m => m.OrderNo)
                .ToList();

            var index = ordered.FindIndex(m => m.ModuleId == moduleId);

            bool isUnlocked;

            if (index == 0)
            {
                isUnlocked = true;
            }
            else
            {
                var prev = ordered[index - 1];

                var prevProgress = await _moduleProgressRepo.GetAsync(
                    userId,
                    prev.ModuleId);

                isUnlocked = prevProgress != null &&
                             prevProgress.Status == "Completed";
            }

            var progress = await _moduleProgressRepo.GetAsync(
                userId,
                moduleId);

            return new ModuleProgressDto
            {
                ModuleId = moduleId,
                ModuleName = module.ModuleName ?? "",
                OrderNo = module.OrderNo ?? 0,
                IsUnlocked = isUnlocked,
                IsCompleted = progress != null &&
                              progress.Status == "Completed"
            };
        }


    }
}
