namespace TrainingPlanParser.Services.Validation
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public double AccuracyScore { get; set; }

        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
