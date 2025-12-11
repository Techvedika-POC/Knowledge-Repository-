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
        private readonly IUserProgressRepository _userProgressRepo;

        public ModuleService(
            IModuleRepository moduleRepo,
            IUserProgressRepository userProgressRepo)
        {
            _moduleRepo = moduleRepo;
            _userProgressRepo = userProgressRepo;
        }

        // CREATE MODULE
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

        // GET MODULES BY WEEK
        public async Task<IEnumerable<ModuleDto>> GetModulesByWeekAsync(Guid weekId)
        {
            var modules = await _moduleRepo.GetByWeekIdAsync(weekId);

            return modules.Select(m => new ModuleDto
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
                Metadata = m.Metadata ?? ""
            }).ToList();
        }

        // GET FULL MODULE DETAILS 
        public async Task<ModuleDetailDto?> GetModuleDetailAsync(Guid moduleId, Guid userId)
        {
            var module = await _moduleRepo.GetModuleFullAsync(moduleId);
            if (module == null) return null;

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
                    .Select(l => new LessonDto
                    {
                        LessonId = l.LessonId,
                        ModuleId = l.ModuleId,
                        Title = l.Title ?? "",
                        Content = l.Content ?? "",
                        LessonType = l.LessonType ?? "Text",
                        OrderIndex = l.OrderIndex ?? 0,
                        Overview = l.Overview ?? "",
                        Prerequisites = l.Prerequisites ?? "",
                        DurationMinutes = l.DurationMinutes
                    }).ToList(),

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
                    }).ToList(),

                Assessments = module.Assessments
                    .OrderBy(a => a.CreatedOn)
                    .Select(a => new AssessmentDto
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
                            }).ToList()
                    }).ToList()
            };
        }

        // UPDATE MODULE
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

        // DELETE MODULE
        public async Task DeleteModuleAsync(Guid moduleId)
        {
            var module = await _moduleRepo.GetByIdAsync(moduleId);
            if (module == null) throw new Exception("Module not found");

            await _moduleRepo.DeleteAsync(module);
        }

        // GET MODULE PROGRESS
        public async Task<ModuleProgressDto?> GetModuleProgressAsync(Guid moduleId, Guid userId)
        {
            var module = await _moduleRepo.GetByIdAsync(moduleId);
            if (module == null) return null;

            var progress = await _userProgressRepo.GetModuleProgressAsync(userId, module.WeekId, moduleId);

            return new ModuleProgressDto
            {
                ModuleId = moduleId,
                ModuleName = module.ModuleName ?? "",
                OrderNo = module.OrderNo ?? 0,
                IsUnlocked = progress != null,
                IsCompleted = progress?.Status == "Completed"
            };
        }
    }
}
