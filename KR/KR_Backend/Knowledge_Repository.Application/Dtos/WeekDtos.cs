namespace Knowledge_Repository.Application.Dtos
{
    public class WeekDto
    {
        public Guid WeekId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int WeekNumber { get; set; }
        public string LearningObjectives { get; set; } = string.Empty;
        public string Prerequisites { get; set; } = string.Empty;
    }

    public class WeekProgressDto
    {
        public Guid WeekId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int WeekNumber { get; set; }
        public bool IsUnlocked { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public int TotalModules { get; set; }
        public int CompletedModules { get; set; }
    }
}
