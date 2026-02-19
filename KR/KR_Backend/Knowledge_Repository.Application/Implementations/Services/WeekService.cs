using Application.Interfaces;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class WeekService : IWeekService
    {
        private readonly IWeekRepository _weekRepo;
        private readonly IUserModuleProgressRepository _moduleProgressRepo;

        public WeekService(
            IWeekRepository weekRepo,
            IUserModuleProgressRepository moduleProgressRepo)
        {
            _weekRepo = weekRepo;
            _moduleProgressRepo = moduleProgressRepo;
        }
        public async Task<WeekDto> CreateWeekAsync(Guid planId, WeekDto weekDto)
        {
            if (weekDto == null) throw new ArgumentNullException(nameof(weekDto));

            var week = new Week
            {
                WeekId = Guid.NewGuid(),
                PlanId = planId,
                WeekNumber = weekDto.WeekNumber,
                Description = weekDto.Title,
                LearningObjectives = weekDto.LearningObjectives,
                Prerequisites = weekDto.Prerequisites,
                CreatedOn = DateTime.Now
            };

            await _weekRepo.AddAsync(week);

            weekDto.WeekId = week.WeekId;
            return weekDto;
        }

        public async Task<IEnumerable<WeekDto>> GetWeeksByPlanAsync(Guid planId)
        {
            var weeks = await _weekRepo.GetByPlanIdAsync(planId);

            return weeks.OrderBy(w => w.WeekNumber).Select(w => new WeekDto
            {
                WeekId = w.WeekId,
                Title = w.Description,
                WeekNumber = w.WeekNumber,
                LearningObjectives = w.LearningObjectives,
                Prerequisites = w.Prerequisites
            });
        }

        public async Task<WeekDto?> GetWeekByIdAsync(Guid weekId)
        {
            var week = await _weekRepo.GetByIdAsync(weekId);
            if (week == null) return null;

            return new WeekDto
            {
                WeekId = week.WeekId,
                Title = week.Description,
                WeekNumber = week.WeekNumber,
                LearningObjectives = week.LearningObjectives,
                Prerequisites = week.Prerequisites
            };
        }
        public async Task<WeekProgressDto?> GetWeekProgressAsync(Guid weekId, Guid userId)
        {
            var week = await _weekRepo.GetWeekWithModulesAsync(weekId);
            if (week == null) return null;

            int totalModules = week.Modules.Count;
            int completedModules = 0;

            foreach (var module in week.Modules)
            {
                var progress = await _moduleProgressRepo.GetAsync(
                    userId,
                    module.ModuleId);

                if (progress != null &&
                    string.Equals(
                        progress.Status,
                        "Completed",
                        StringComparison.OrdinalIgnoreCase))
                {
                    completedModules++;
                }
            }

            return new WeekProgressDto
            {
                WeekId = week.WeekId,
                Title = week.Description,
                WeekNumber = week.WeekNumber,
                TotalModules = totalModules,
                CompletedModules = completedModules,
                IsUnlocked = await _weekRepo.IsWeekUnlockedAsync(
                    week.WeekId,
                    userId),
                IsCompleted = completedModules == totalModules
            };
        }

        public async Task UpdateWeekAsync(Guid weekId, WeekDto weekDto)
        {
            var week = await _weekRepo.GetByIdAsync(weekId);
            if (week == null) throw new KeyNotFoundException("Week not found.");

            week.Description = weekDto.Title;
            week.WeekNumber = weekDto.WeekNumber;
            week.LearningObjectives = weekDto.LearningObjectives;
            week.Prerequisites = weekDto.Prerequisites;
            week.UpdatedOn = DateTime.Now;

            await _weekRepo.UpdateAsync(week);
        }
        public async Task DeleteWeekAsync(Guid weekId)
        {
            var week = await _weekRepo.GetByIdAsync(weekId);
            if (week == null) throw new KeyNotFoundException("Week not found.");

            await _weekRepo.DeleteAsync(week);
        }

        public async Task<WeekFullDto?> GetWeekFullByIdAsync(Guid weekId, Guid? userId = null)
        {
            var week = await _weekRepo.GetWeekWithModulesAsync(weekId);
            if (week == null) return null;

            var dto = new WeekFullDto
            {
                WeekId = week.WeekId,
                Title = week.Description,
                WeekNumber = week.WeekNumber,
                Overview = week.Description ?? "",
                LearningObjectives = week.LearningObjectives ?? "",
                Prerequisites = week.Prerequisites ?? "",

                Modules = week.Modules.Select(m => new ModuleDetailDto
                {
                    ModuleId = m.ModuleId,
                    ModuleName = m.ModuleName,
                    Description = m.Description,
                    OrderNo = m.OrderNo ?? 0,

                    Lessons = m.Lessons.Select(l => new LessonDto
                    {
                        LessonId = l.LessonId,
                        ModuleId = l.ModuleId,
                        Title = l.Title,
                        Content = l.Content,
                        LessonType = l.LessonType,
                        OrderIndex = l.OrderIndex ?? 0
                    }).ToList(),

                    Resources = m.Resources.Select(r => new ResourceDto
                    {
                        ResourceId = r.ResourceId,
                        ModuleId = r.ModuleId,
                        Title = r.Title,
                        Url = r.Url,
                        ResourceType = r.ResourceType,
                        TopicId = r.TopicId
                    }).ToList(),

                    Assessments = m.Assessments.Select(a => new AssessmentDto
                    {
                        AssessmentId = a.AssessmentId,
                        ModuleId = a.ModuleId ?? Guid.Empty,
                        TopicId = a.TopicId,
                        Title = a.Title ?? "",
                        Difficulty = a.Difficulty ?? 0,

                        Questions = (a.AssessmentQuestions ?? Enumerable.Empty<AssessmentQuestion>())
                            .Select(q => new AssessmentQuestionDto
                            {
                                QuestionId = q.QuestionId,
                                Question = q.Question ?? "",
                                Options = q.Options ?? "",
                                CorrectAnswer = q.CorrectAnswer ?? "",
                                Explanation = q.Explanation ?? ""
                            }).ToList()
                    }).ToList()
                }).ToList()
            };

            if (userId.HasValue)
            {
                dto.IsUnlocked = await _weekRepo.IsWeekUnlockedAsync(
                    week.WeekId,
                    userId.Value);

                bool allCompleted = true;

                foreach (var module in week.Modules)
                {
                    var progress = await _moduleProgressRepo.GetAsync(
                        userId.Value,
                        module.ModuleId);

                    if (progress == null ||
                        !string.Equals(
                            progress.Status,
                            "Completed",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        allCompleted = false;
                        break;
                    }
                }

                dto.IsCompleted = allCompleted;
            }

            return dto;
        }

        public async Task<IEnumerable<WeekFullDto>> GetWeeksFullByPlanAsync(Guid planId, Guid? userId = null)
        {
            var weeks = await _weekRepo.GetByPlanIdAsync(planId);
            var result = new List<WeekFullDto>();

            foreach (var week in weeks.OrderBy(w => w.WeekNumber))
            {
                var fullWeek = await GetWeekFullByIdAsync(week.WeekId, userId);
                if (fullWeek != null)
                    result.Add(fullWeek);
            }

            return result;
        }
    }
}
