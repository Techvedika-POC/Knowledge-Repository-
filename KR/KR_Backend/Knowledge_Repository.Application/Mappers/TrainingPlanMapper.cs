using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Application.Dtos;

namespace Knowledge_Repository.Application.Mappers
{
    public static class TrainingPlanMapper
    {
        public static LearningPlan ToEntity(TrainingPlanDto dto, Guid createdBy)
        {
            var planId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var plan = new LearningPlan
            {
                PlanId = planId,
                Title = dto.Title,
                Description = dto.Description,
                Overview = dto.Overview,
                Objectives = dto.Objectives,
                DurationWeeks = dto.DurationWeeks,
                TotalDays = dto.TotalDays,
                Prerequisites = string.Join(";", dto.Prerequisites ?? []),
                TechnicalRequirements = string.Join(";", dto.TechnicalRequirements ?? []),
                SelfAssessmentChecklist = string.Join(";", dto.SelfAssessmentChecklist ?? []),
                CreatedBy = createdBy,
                CreatedOn = now,
                IsDynamicGenerated = true
            };

            foreach (var weekDto in dto.Weeks)
            {
                var weekId = Guid.NewGuid();

                var week = new Week
                {
                    WeekId = weekId,
                    PlanId = planId,
                    WeekNumber = weekDto.WeekNumber,
                    Description = weekDto.Description,
                    Prerequisites = string.Join(";", weekDto.Prerequisites ?? []),
                    LearningObjectives = string.Join(";", weekDto.LearningObjectives ?? []),
                    Metadata = weekDto.Metadata,
                    IsAiGenerated = true,
                    CreatedOn = now
                };

                foreach (var moduleDto in weekDto.Modules)
                {
                    var moduleId = Guid.NewGuid();

                    var module = new Module
                    {
                        ModuleId = moduleId,
                        WeekId = weekId,
                        ModuleName = moduleDto.ModuleName,
                        Description = moduleDto.Description,
                        Overview = moduleDto.Overview,
                        OrderNo = moduleDto.OrderNo,
                        DurationDays = moduleDto.DurationDays,
                        Prerequisites = string.Join(";", moduleDto.Prerequisites ?? []),
                        Metadata = moduleDto.Metadata,
                        IsAiGenerated = true,
                        CreatedOn = now
                    };

                    foreach (var lessonDto in moduleDto.Lessons)
                    {
                        var lessonId = Guid.NewGuid();

                        var lesson = new Lesson
                        {
                            LessonId = lessonId,
                            ModuleId = moduleId,
                            Title = lessonDto.Title,
                            Content = lessonDto.Content,
                            LessonType = lessonDto.LessonType,
                            OrderIndex = lessonDto.OrderIndex,
                            Overview = lessonDto.Overview,
                            Prerequisites = string.Join(";", lessonDto.Prerequisites ?? []),
                            DurationMinutes = lessonDto.DurationMinutes,
                            Metadata = lessonDto.Metadata,
                            IsAiGenerated = true,
                            CreatedOn = now
                        };

                        module.Lessons.Add(lesson);

                        // ✅ RESOURCES (LESSON-SCOPED)
                        foreach (var resourceDto in lessonDto.Resources ?? [])
                        {
                            module.Resources.Add(new Resource
                            {
                                ResourceId = Guid.NewGuid(),
                                ModuleId = moduleId,
                                TopicId = lessonId, // 🔥 REQUIRED
                                Title = resourceDto.Title,
                                Url = resourceDto.Url,
                                ResourceType = resourceDto.ResourceType,
                                Description = resourceDto.Description,
                                Metadata = resourceDto.Metadata,
                                IsAiGenerated = true,
                                CreatedOn = now
                            });
                        }

                        // ✅ ASSESSMENTS (LESSON-SCOPED)
                        foreach (var assessmentDto in lessonDto.Assessments ?? [])
                        {
                            module.Assessments.Add(new Assessment
                            {
                                AssessmentId = Guid.NewGuid(),
                                ModuleId = moduleId,
                                TopicId = lessonId, // 🔥 REQUIRED
                                Title = assessmentDto.Title,
                                AssessmentType = assessmentDto.AssessmentType,
                                Difficulty = assessmentDto.Difficulty,
                                EstimatedDurationMinutes = assessmentDto.EstimatedDurationMinutes,
                                Description = assessmentDto.Description,
                                Metadata = assessmentDto.Metadata,
                                IsAiGenerated = true,
                                CreatedOn = now
                            });
                        }
                    }

                    week.Modules.Add(module);
                }

                foreach (var assignmentDto in weekDto.Assignments ?? [])
                {
                    week.Assignments.Add(new Assignment
                    {
                        AssignmentId = Guid.NewGuid(),
                        WeekId = weekId,
                        Title = assignmentDto.Title,
                        Description = assignmentDto.Description,
                        CreatedOn = now
                    });
                }

                plan.Weeks.Add(week);
            }

            return plan;
        }

    }
}
