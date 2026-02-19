using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos
{
    public class ModuleStatusDto
    {
        public Guid ModuleId { get; set; }
        public string ModuleTitle { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public string TestStatus { get; set; } = "NotTaken";
        public decimal LessonProgressPercent { get; set; } = 0m;
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
    }

    public class UserProgressDto
    {
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public List<ModuleStatusDto> Modules { get; set; } = new();
    }
}
