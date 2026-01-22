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
    public class LearningPlanService : ILearningPlanService
    {
        private readonly ILearningPlanRepository _planRepo;
        private readonly IWeekRepository _weekRepo;
        private readonly IModuleRepository _moduleRepo;
        private readonly ILessonRepository _lessonRepo;
        private readonly IResourceRepository _resourceRepo;
        private readonly IAssessmentRepository _assessmentRepo;
        private readonly IUserProgressRepository _userProgressRepo;

        public LearningPlanService(
            ILearningPlanRepository planRepo,
            IWeekRepository weekRepo,
            IModuleRepository moduleRepo,
            ILessonRepository lessonRepo,
            IResourceRepository resourceRepo,
            IAssessmentRepository assessmentRepo,
            IUserProgressRepository userProgressRepo)
        {
            _planRepo = planRepo ?? throw new ArgumentNullException(nameof(planRepo));
            _weekRepo = weekRepo ?? throw new ArgumentNullException(nameof(weekRepo));
            _moduleRepo = moduleRepo ?? throw new ArgumentNullException(nameof(moduleRepo));
            _lessonRepo = lessonRepo ?? throw new ArgumentNullException(nameof(lessonRepo));
            _resourceRepo = resourceRepo ?? throw new ArgumentNullException(nameof(resourceRepo));
            _assessmentRepo = assessmentRepo ?? throw new ArgumentNullException(nameof(assessmentRepo));
            _userProgressRepo = userProgressRepo ?? throw new ArgumentNullException(nameof(userProgressRepo));
        }


        public async Task<List<LearningPlanDto>> GetAllPlansAsync()
        {
            var plans = (await _planRepo.GetAllAsync())
                .OrderByDescending(p => p.UpdatedOn ?? p.CreatedOn)
                .ToList();

            return plans.Select(x => new LearningPlanDto
            {
                PlanId = x.PlanId,
                Title = x.Title ?? string.Empty,
                Description = x.Description ?? string.Empty,
                DurationWeeks = x.DurationWeeks,
                TotalDays = x.TotalDays,
                Overview = x.Overview ?? string.Empty,
                Objectives = x.Objectives ?? string.Empty,
                Prerequisites = x.Prerequisites ?? string.Empty,
                TechnicalRequirements = x.TechnicalRequirements ?? string.Empty,
                SelfAssessmentChecklist = x.SelfAssessmentChecklist ?? string.Empty,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                Weeks = new List<WeekFullDto>()
            }).ToList();
        }


        public async Task<LearningPlanDto?> GetPlanHierarchyAsync(Guid planId)
        {
            var plan = await _planRepo.GetPlanWithHierarchyAsync(planId);
            if (plan == null) return null;

            var weeks = plan.Weeks?
                .OrderBy(w => w.WeekNumber)
                .Select(w => new WeekFullDto
                {
                    WeekId = w.WeekId,
                    Title = w.Description ?? string.Empty,
                    WeekNumber = w.WeekNumber,
                    Modules = new List<ModuleDetailDto>()
                }).ToList() ?? new List<WeekFullDto>();

            return new LearningPlanDto
            {
                PlanId = plan.PlanId,
                Title = plan.Title,
                Weeks = weeks
            };
        }

        public async Task<LearningPlanFullDto?> GetPlanHierarchyFullAsync(Guid planId, Guid? userId = null)
        {
            var plan = await _planRepo.GetPlanWithHierarchyFullAsync(planId);
            if (plan == null) return null;

            var planDto = new LearningPlanFullDto
            {
                PlanId = plan.PlanId,
                Title = plan.Title ?? string.Empty,
                Description = plan.Description ?? string.Empty,
                DurationWeeks = plan.DurationWeeks,
                TotalDays = plan.TotalDays,
                Overview = plan.Overview ?? string.Empty,
                Objectives = plan.Objectives ?? string.Empty,
                Prerequisites = plan.Prerequisites ?? string.Empty,
                TechnicalRequirements = plan.TechnicalRequirements ?? string.Empty,
                SelfAssessmentChecklist = plan.SelfAssessmentChecklist ?? string.Empty,
                Weeks = (plan.Weeks ?? Enumerable.Empty<Week>())
                    .OrderBy(w => w.WeekNumber)
                    .Select(w => new WeekFullDto
                    {
                        WeekId = w.WeekId,
                        Title = w.Description ?? string.Empty,
                        WeekNumber = w.WeekNumber,
                        Overview = w.Description ?? string.Empty,
                        LearningObjectives = w.LearningObjectives ?? string.Empty,
                        Prerequisites = w.Prerequisites ?? string.Empty,
                        Modules = (w.Modules ?? Enumerable.Empty<Module>())
                            .OrderBy(m => m.OrderNo)
                            .Select(m => new ModuleDetailDto
                            {
                                ModuleId = m.ModuleId,
                                ModuleName = m.ModuleName ?? string.Empty,
                                Description = m.Description ?? string.Empty,
                                OrderNo = m.OrderNo ?? 0,
                                Lessons = (m.Lessons ?? Enumerable.Empty<Lesson>())
                                    .OrderBy(l => l.OrderIndex)
                                    .Select(l => new LessonDto
                                    {
                                        LessonId = l.LessonId,
                                        ModuleId = l.ModuleId,
                                        Title = l.Title ?? string.Empty,
                                        Content = l.Content ?? string.Empty,
                                        LessonType = l.LessonType ?? string.Empty,
                                        OrderIndex = l.OrderIndex ?? 0
                                    }).ToList(),
                                Resources = (m.Resources ?? Enumerable.Empty<Resource>())
                                    .Select(r => new ResourceDto
                                    {
                                        ResourceId = r.ResourceId,
                                        Title = r.Title ?? string.Empty,
                                        Url = r.Url ?? string.Empty,
                                        ResourceType = r.ResourceType ?? string.Empty,
                                        ModuleId = r.ModuleId,
                                        TopicId = r.TopicId
                                    }).ToList(),
                                Assessments = (m.Assessments ?? Enumerable.Empty<Assessment>())
                                    .Select(a => new AssessmentDto
                                    {
                                        AssessmentId = a.AssessmentId,
                                        ModuleId = a.ModuleId ?? Guid.Empty,
                                        TopicId = a.TopicId,
                                        Title = a.Title ?? string.Empty,
                                        Difficulty = a.Difficulty ?? 0,
                                        Metadata = a.Metadata ?? string.Empty,
                                        Questions = (a.AssessmentQuestions ?? Enumerable.Empty<AssessmentQuestion>())
                                            .Select(q => new AssessmentQuestionDto
                                            {
                                                QuestionId = q.QuestionId,
                                                Question = q.Question ?? string.Empty,
                                                Options = q.Options ?? string.Empty,
                                                CorrectAnswer = q.CorrectAnswer ?? string.Empty,
                                                Explanation = q.Explanation ?? string.Empty
                                            }).ToList()
                                    }).ToList(),
                            }).ToList()
                    }).ToList()
            };

            if (userId.HasValue)
            {
                var weekList = planDto.Weeks.ToList();
                foreach (var week in weekList)
                {
                    bool anyUnlocked = false;
                    bool allCompleted = true;

                    foreach (var module in week.Modules ?? new List<ModuleDetailDto>())
                    {
                        var progress = await _userProgressRepo.GetModuleProgressAsync(userId.Value, planId, module.ModuleId);
                        if (progress != null)
                        {
                            anyUnlocked = true;
                            if (!string.Equals(progress.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                                allCompleted = false;
                        }
                        else
                        {
                            allCompleted = false;
                        }
                    }

                    week.IsUnlocked = anyUnlocked;
                    week.IsCompleted = allCompleted;
                }

                planDto.Weeks = weekList;
            }

            return planDto;
        }

        public async Task<bool> IsPlanCompletedAsync(Guid planId, Guid userId)
            => await _planRepo.IsPlanCompletedByUser(planId, userId);

  
        public async Task<LearningPlanDto> GenerateLearningPlanAsync(string title, bool useAI)
        {
            var plan = new LearningPlan
            {
                PlanId = Guid.NewGuid(),
                Title = title,
                CreatedOn = DateTime.Now
            };

            await _planRepo.AddAsync(plan);

            return new LearningPlanDto
            {
                PlanId = plan.PlanId,
                Title = plan.Title
            };
        }

        public async Task<LearningPlanFullDto> CreateFullLearningPlanAsync(LearningPlanFullDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var planEntity = new LearningPlan
            {
                PlanId = dto.PlanId == Guid.Empty ? Guid.NewGuid() : dto.PlanId,
                Title = dto.Title,
                Description = dto.Description,
                DurationWeeks = dto.DurationWeeks,
                TotalDays = dto.TotalDays,
                Overview = dto.Overview,
                Objectives = dto.Objectives,
                Prerequisites = dto.Prerequisites,
                TechnicalRequirements = dto.TechnicalRequirements,
                SelfAssessmentChecklist = dto.SelfAssessmentChecklist,
                CreatedOn = DateTime.Now
            };

            await _planRepo.AddAsync(planEntity);

            var createdWeeks = new List<WeekFullDto>();

            foreach (var wDto in dto.Weeks ?? Enumerable.Empty<WeekFullDto>())
            {
                var weekEntity = new Week
                {
                    WeekId = wDto.WeekId == Guid.Empty ? Guid.NewGuid() : wDto.WeekId,
                    PlanId = planEntity.PlanId,
                    WeekNumber = wDto.WeekNumber,
                    Description = string.IsNullOrWhiteSpace(wDto.Overview) ? wDto.Title : wDto.Overview,
                    LearningObjectives = wDto.LearningObjectives,
                    Prerequisites = wDto.Prerequisites,
                    CreatedOn = DateTime.Now
                };

                await _weekRepo.AddAsync(weekEntity);

                var createdModules = new List<ModuleDetailDto>();

                foreach (var mDto in wDto.Modules ?? Enumerable.Empty<ModuleDetailDto>())
                {
                    var moduleEntity = new Module
                    {
                        ModuleId = mDto.ModuleId == Guid.Empty ? Guid.NewGuid() : mDto.ModuleId,
                        WeekId = weekEntity.WeekId,
                        ModuleName = mDto.ModuleName,
                        Description = mDto.Description,
                        OrderNo = mDto.OrderNo,
                        CreatedOn = DateTime.Now
                    };

                    await _moduleRepo.AddAsync(moduleEntity);

                    foreach (var lDto in mDto.Lessons ?? Enumerable.Empty<LessonDto>())
                    {
                        var lessonEntity = new Lesson
                        {
                            LessonId = lDto.LessonId == Guid.Empty ? Guid.NewGuid() : lDto.LessonId,
                            ModuleId = moduleEntity.ModuleId,
                            Title = lDto.Title,
                            Content = lDto.Content,
                            LessonType = lDto.LessonType,
                            OrderIndex = lDto.OrderIndex,
                            CreatedOn = DateTime.Now
                        };

                        await _lessonRepo.AddAsync(lessonEntity);
                    }

                    foreach (var rDto in mDto.Resources ?? Enumerable.Empty<ResourceDto>())
                    {
                        var resourceEntity = new Resource
                        {
                            ResourceId = rDto.ResourceId == Guid.Empty ? Guid.NewGuid() : rDto.ResourceId,
                            ModuleId = moduleEntity.ModuleId,
                            Title = rDto.Title,
                            Url = rDto.Url,
                            ResourceType = rDto.ResourceType,
                            TopicId = rDto.TopicId,
                            CreatedOn = DateTime.Now
                        };

                        await _resourceRepo.AddAsync(resourceEntity);
                    }

                    foreach (var aDto in mDto.Assessments ?? Enumerable.Empty<AssessmentDto>())
                    {
                        var assessmentEntity = new Assessment
                        {
                            AssessmentId = aDto.AssessmentId == Guid.Empty ? Guid.NewGuid() : aDto.AssessmentId,
                            ModuleId = moduleEntity.ModuleId,
                            TopicId = aDto.TopicId,
                            Title = aDto.Title,
                            Difficulty = aDto.Difficulty,
                            Metadata = aDto.Metadata,
                            CreatedOn = DateTime.Now
                        };

                        await _assessmentRepo.AddAsync(assessmentEntity);

                        if (aDto.Questions != null && aDto.Questions.Any())
                        {
                            var qEntities = aDto.Questions.Select(qDto => new AssessmentQuestion
                            {
                                QuestionId = qDto.QuestionId == Guid.Empty ? Guid.NewGuid() : qDto.QuestionId,
                                AssessmentId = assessmentEntity.AssessmentId,
                                Question = qDto.Question,
                                Options = qDto.Options,
                                CorrectAnswer = qDto.CorrectAnswer,
                                Explanation = qDto.Explanation,
                                CreatedOn = DateTime.Now
                            }).ToList();

                            await _assessmentRepo.AddQuestionsAsync(qEntities);
                        }
                    }

                    createdModules.Add(new ModuleDetailDto
                    {
                        ModuleId = moduleEntity.ModuleId,
                        ModuleName = moduleEntity.ModuleName ?? string.Empty,
                        Description = moduleEntity.Description ?? string.Empty,
                        OrderNo = moduleEntity.OrderNo ?? 0,
                        Lessons = (mDto.Lessons ?? Enumerable.Empty<LessonDto>()).ToList(),
                        Resources = (mDto.Resources ?? Enumerable.Empty<ResourceDto>()).ToList(),
                        Assessments = (mDto.Assessments ?? Enumerable.Empty<AssessmentDto>()).ToList()
                    });
                }

                createdWeeks.Add(new WeekFullDto
                {
                    WeekId = weekEntity.WeekId,
                    Title = wDto.Title ?? weekEntity.Description,
                    WeekNumber = weekEntity.WeekNumber,
                    Overview = weekEntity.Description,
                    LearningObjectives = weekEntity.LearningObjectives ?? string.Empty,
                    Prerequisites = weekEntity.Prerequisites ?? string.Empty,
                    Modules = createdModules
                });
            }

            return new LearningPlanFullDto
            {
                PlanId = planEntity.PlanId,
                Title = planEntity.Title,
                Description = planEntity.Description ?? string.Empty,
                DurationWeeks = planEntity.DurationWeeks,
                TotalDays = planEntity.TotalDays,
                Overview = planEntity.Overview ?? string.Empty,
                Objectives = planEntity.Objectives ?? string.Empty,
                Prerequisites = planEntity.Prerequisites ?? string.Empty,
                TechnicalRequirements = planEntity.TechnicalRequirements ?? string.Empty,
                SelfAssessmentChecklist = planEntity.SelfAssessmentChecklist ?? string.Empty,
                Weeks = createdWeeks
            };
        }

        public async Task<bool> UpdateLearningPlanAsync(LearningPlanFullDto dto)
        {
            var plan = await _planRepo.GetByIdAsync(dto.PlanId);
            if (plan == null) return false;

            plan.Title = dto.Title;
            plan.Description = dto.Description;
            plan.DurationWeeks = dto.DurationWeeks;
            plan.TotalDays = dto.TotalDays;
            plan.Overview = dto.Overview;
            plan.Objectives = dto.Objectives;
            plan.Prerequisites = dto.Prerequisites;
            plan.TechnicalRequirements = dto.TechnicalRequirements;
            plan.SelfAssessmentChecklist = dto.SelfAssessmentChecklist;
            plan.UpdatedOn = DateTime.Now;

            await _planRepo.UpdateAsync(plan);
            return true;
        }

        public async Task<bool> UpdateWeekAsync(WeekFullDto dto)
        {
            var week = await _weekRepo.GetByIdAsync(dto.WeekId);
            if (week == null) return false;

            week.Description = dto.Title;
            week.WeekNumber = dto.WeekNumber;
            week.LearningObjectives = dto.LearningObjectives;
            week.Prerequisites = dto.Prerequisites;
            week.UpdatedOn = DateTime.Now;

            await _weekRepo.UpdateAsync(week);
            return true;
        }

        public async Task<bool> UpdateModuleAsync(ModuleDetailDto dto)
        {
            var module = await _moduleRepo.GetByIdAsync(dto.ModuleId);
            if (module == null) return false;

            module.ModuleName = dto.ModuleName;
            module.Description = dto.Description;
            module.OrderNo = dto.OrderNo;
            module.UpdatedOn = DateTime.Now;

            await _moduleRepo.UpdateAsync(module);
            return true;
        }

        public async Task<bool> UpdateLessonAsync(LessonDto dto)
        {
            var lesson = await _lessonRepo.GetByIdAsync(dto.LessonId);
            if (lesson == null) return false;

            lesson.Title = dto.Title;
            lesson.Content = dto.Content;
            lesson.LessonType = dto.LessonType;
            lesson.OrderIndex = dto.OrderIndex;
            lesson.UpdatedOn = DateTime.Now;

            await _lessonRepo.UpdateAsync(lesson);
            return true;
        }

    
        public async Task<bool> DeleteLearningPlanAsync(Guid planId)
        {
            var plan = await _planRepo.GetByIdAsync(planId);
            if (plan == null) return false;

            await _planRepo.DeleteAsync(plan);
            return true;
        }

        public async Task<bool> DeleteModuleAsync(Guid moduleId)
        {
            var module = await _moduleRepo.GetByIdAsync(moduleId);
            if (module == null) return false;

            await _moduleRepo.DeleteAsync(module);
            return true;
        }
    }
}
