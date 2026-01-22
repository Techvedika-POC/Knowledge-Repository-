using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class TrainingPlanMappingService : ITrainingPlanMappingService
    {
        private static string SafeJson(string? value)
            => string.IsNullOrWhiteSpace(value) ? "{}" : value;

        public LearningPlan MapToLearningPlan(
            TrainingPlanIngestionDto dto,
            Guid createdBy)
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
                    Metadata = SafeJson(weekDto.Metadata),
                    CreatedOn = now,
                    IsAiGenerated = true
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
                        Metadata = SafeJson(moduleDto.Metadata),
                        CreatedOn = now,
                        IsAiGenerated = true
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
                            Metadata = SafeJson(lessonDto.Metadata),
                            CreatedOn = now,
                            IsAiGenerated = true
                        };

                        module.Lessons.Add(lesson);

                        foreach (var resourceDto in lessonDto.Resources ?? [])
                        {
                            module.Resources.Add(new Resource
                            {
                                ResourceId = Guid.NewGuid(),
                                ModuleId = moduleId,
                                TopicId = lessonId, 
                                Title = resourceDto.Title,
                                Url = resourceDto.Url,
                                ResourceType = resourceDto.ResourceType,
                                Description = resourceDto.Description,
                                Metadata = SafeJson(resourceDto.Metadata),
                                CreatedOn = now,
                                IsAiGenerated = true
                            });
                        }

                        foreach (var assessmentDto in lessonDto.Assessments ?? [])
                        {
                            var assessmentId = Guid.NewGuid();

                            var assessment = new Assessment
                            {
                                AssessmentId = assessmentId,
                                ModuleId = moduleId,
                                TopicId = lessonId, 
                                Title = assessmentDto.Title,
                                AssessmentType = assessmentDto.AssessmentType,
                                Difficulty = assessmentDto.Difficulty,
                                EstimatedDurationMinutes = assessmentDto.EstimatedDurationMinutes,
                                LearningObjectives = assessmentDto.LearningObjectives,
                                Description = assessmentDto.Description,
                                Metadata = SafeJson(assessmentDto.Metadata),
                                CreatedOn = now,
                                IsAiGenerated = true
                            };

                            foreach (var q in assessmentDto.Questions ?? [])
                            {
                                assessment.AssessmentQuestions.Add(new AssessmentQuestion
                                {
                                    QuestionId = Guid.NewGuid(),
                                    AssessmentId = assessmentId,
                                    Question = q.Question,
                                    QuestionType = q.QuestionType,
                                    Options = SafeJson(q.Options),
                                    CorrectAnswer = q.CorrectAnswer,
                                    Explanation = q.Explanation,
                                    Marks = q.Marks,
                                    EvaluationStrategy = q.EvaluationStrategy,
                                    Metadata = SafeJson(q.Metadata),
                                    CreatedOn = now,
                                    IsAiGenerated = true
                                });
                            }

                            module.Assessments.Add(assessment);
                        }

                    }

                    week.Modules.Add(module);
                }

                foreach (var assignmentDto in weekDto.Assignments)
                {
                    week.Assignments.Add(new Assignment
                    {
                        AssignmentId = Guid.NewGuid(),
                        WeekId = weekId,
                        Title = assignmentDto.Title,
                        Description = assignmentDto.Description,
                        EstimatedDurationMinutes = assignmentDto.EstimatedDurationMinutes,
                        AssignmentType = "AI_GENERATED",
                        CreatedOn = now
                    });
                }

                plan.Weeks.Add(week);
            }

            return plan;
        }
    }
}
