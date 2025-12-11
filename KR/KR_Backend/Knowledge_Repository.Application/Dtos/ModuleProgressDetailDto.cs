using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos
{
    public class LessonProgressDto
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string LessonType { get; set; } = "Video";
        public int OrderIndex { get; set; }
        public bool IsCompleted { get; set; } = false;
    }

    public class AssessmentProgressDto
    {
        public Guid AssessmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Difficulty { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public bool IsUnlocked { get; set; } = false;

        // Optional: if later you want to show score %
        public decimal ScorePercent { get; set; } = 0m;
    }

    public class ResourceProgressDto
    {
        public Guid ResourceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ResourceType { get; set; } = "Link";
        public string Url { get; set; } = string.Empty;
    }

    public class ModuleProgressDetailDto
    {
        public Guid ModuleId { get; set; }
        public string ModuleTitle { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; } = false;
        public bool IsCompleted { get; set; } = false;

        // ✅ NEW – detailed lesson progress for this module
        public decimal LessonProgressPercent { get; set; } = 0m;
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }

        public List<LessonProgressDto> Lessons { get; set; } = new();
        public List<AssessmentProgressDto> Assessments { get; set; } = new();
        public List<ResourceProgressDto> Resources { get; set; } = new();
    }

    public class UserPlanProgressDetailDto
    {
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public List<ModuleProgressDetailDto> Modules { get; set; } = new();
    }
}
