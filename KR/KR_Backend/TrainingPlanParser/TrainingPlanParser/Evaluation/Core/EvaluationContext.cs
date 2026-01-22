namespace TrainingPlanParser.Services.Evaluation.Core
{
    public class EvaluationContext
    {
        public string RawText { get; init; }
        public string StructuredText { get; init; }
        public string LlmJson { get; init; }
    }
}
