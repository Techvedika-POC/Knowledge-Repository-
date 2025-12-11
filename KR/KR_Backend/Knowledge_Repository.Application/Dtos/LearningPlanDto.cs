using Knowledge_Repository.Application.Dtos;

public class LearningPlanDto
{
    public Guid PlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; }
    public int? DurationWeeks { get; set; }
    public int? TotalDays { get; set; }
    public string Overview { get; set; }
    public string Objectives { get; set; }
    public string Prerequisites { get; set; }
    public string TechnicalRequirements { get; set; }
    public string SelfAssessmentChecklist { get; set; }

    public List<WeekFullDto> Weeks { get; set; } = new();
}
