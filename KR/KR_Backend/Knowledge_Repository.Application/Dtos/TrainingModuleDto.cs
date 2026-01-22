using Knowledge_Repository.Application.Dtos;

public class TrainingModuleDto
{
    public string ModuleName { get; set; }
    public string Description { get; set; }
    public string Overview { get; set; }

    public int OrderNo { get; set; }
    public int DurationDays { get; set; }

    public List<string> Prerequisites { get; set; }
    public string Metadata { get; set; }

    // 🔥 ONLY THIS
    public List<TrainingLessonDto> Lessons { get; set; } = new();
}
