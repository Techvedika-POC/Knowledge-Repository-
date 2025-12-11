using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.DTOs
{
    public class AiWeekDto
    {
        public string Title { get; set; }            // maps to Description
        public string Description { get; set; }      // maps to Description
        public string Prerequisites { get; set; }
        public string LearningObjectives { get; set; }
        public List<AiModuleDto> Modules { get; set; } = new List<AiModuleDto>();
        public List<AiAssignmentDto> Assignments { get; set; } = new List<AiAssignmentDto>();
    }

    public class AiModuleDto
    {
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public string Prerequisites { get; set; }
        public int? DurationDays { get; set; }
        public List<AiLessonDto> Lessons { get; set; } = new List<AiLessonDto>();
        public List<AiResourceDto> Resources { get; set; } = new List<AiResourceDto>();
        public List<AiAssessmentDto> Assessments { get; set; } = new List<AiAssessmentDto>();
    }

    public class AiLessonDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Prerequisites { get; set; }
        public int? DurationMinutes { get; set; }
        public string LessonType { get; set; }
    }

    public class AiResourceDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ResourceType { get; set; }
    }

    public class AiAssessmentDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AssessmentType { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public string LearningObjectives { get; set; }
        public List<AiAssessmentQuestionDto> Questions { get; set; } = new List<AiAssessmentQuestionDto>();
    }

    public class AiAssessmentQuestionDto
    {
        public string Question { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
        public string Hint { get; set; }
        public string QuestionType { get; set; }
    }

    public class AiAssignmentDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ExpectedOutput { get; set; }
        public string RubricJson { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
    }
}
