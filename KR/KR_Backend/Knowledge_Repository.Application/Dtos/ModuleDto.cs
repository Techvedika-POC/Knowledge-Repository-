using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;

public class ModuleDto
{
    public Guid ModuleId { get; set; }
    public Guid WeekId { get; set; }

    public string ModuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string ExtendedDescription { get; set; } = string.Empty; 
    public string Prerequisites { get; set; } = string.Empty;

    public int? DurationDays { get; set; }
    public int OrderNo { get; set; } = 0;

    public bool IsAiGenerated { get; set; } = false;
    public string Metadata { get; set; } = string.Empty;
}


public class ModuleDetailDto : ModuleDto
{
    public List<LessonDto> Lessons { get; set; } = new();
    public List<ResourceDto> Resources { get; set; } = new();
    public List<AssessmentDto> Assessments { get; set; } = new();
}

public class ModuleProgressDto
{
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int OrderNo { get; set; }
    public bool IsUnlocked { get; set; } = false;
    public bool IsCompleted { get; set; } = false;
}