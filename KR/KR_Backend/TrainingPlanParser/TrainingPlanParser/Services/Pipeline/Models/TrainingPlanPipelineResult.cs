using TrainingPlanParser.Services.Evaluation.Core;
using TrainingPlanParser.Services.Evaluation.RuleBased;

namespace TrainingPlanParser.Services.Pipeline.Models
{
    public class TrainingPlanPipelineResult
    {
        public string RawText { get; set; } = string.Empty;
        public string StructuredText { get; set; } = string.Empty;
        public string LlmJson { get; set; } = string.Empty;

        public bool IsValid { get; set; }
        public decimal FinalScore { get; set; }
        public RuleBasedEvaluationResult RuleBased { get; set; } = new();
        public EvaluationResult ML { get; set; } = new();
        public EvaluationResult Hybrid { get; set; } = new();
    }
}
