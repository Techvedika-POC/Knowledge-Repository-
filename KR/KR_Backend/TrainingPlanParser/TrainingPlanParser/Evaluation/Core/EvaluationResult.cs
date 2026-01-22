namespace TrainingPlanParser.Services.Evaluation.Core
{
    public class EvaluationResult
    {
        public bool IsValid { get; set; }
        public double Score { get; set; }

        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();
        public Dictionary<string, double> Metrics { get; } = new();
    }
}
