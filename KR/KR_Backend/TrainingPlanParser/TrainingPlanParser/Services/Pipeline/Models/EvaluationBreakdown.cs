using TrainingPlanParser.Services.Evaluation.Core;
using TrainingPlanParser.Services.Evaluation.RuleBased;

namespace TrainingPlanParser.Services.Pipeline.Models
{
    public class EvaluationBreakdown
    {
        public RuleBasedEvaluationResult RuleBased { get; set; } = default!;
        public EvaluationResult ML { get; set; } = default!;
        public EvaluationResult Hybrid { get; set; } = default!;
    }
}
