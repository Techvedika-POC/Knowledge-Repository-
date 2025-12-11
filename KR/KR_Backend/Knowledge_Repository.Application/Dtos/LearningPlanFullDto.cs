// File: Knowledge_Repository.Application.Dtos/LearningPlanFullDto.cs
using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos
{
    public class LearningPlanFullDto
    {
        public Guid PlanId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int? DurationWeeks { get; set; }
        public int? TotalDays { get; set; }

        public string Overview { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Prerequisites { get; set; } = string.Empty;
        public string TechnicalRequirements { get; set; } = string.Empty;
        public string SelfAssessmentChecklist { get; set; } = string.Empty;
        public List<WeekFullDto> Weeks { get; set; } = new();
    }

    public class WeekFullDto
    {
        public Guid WeekId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int WeekNumber { get; set; }

        public string Overview { get; set; } = string.Empty;
        public string LearningObjectives { get; set; } = string.Empty;
        public string Prerequisites { get; set; } = string.Empty;
        public int? DurationDays { get; set; }

        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public List<ModuleDetailDto> Modules { get; set; } = new();
    }




}
