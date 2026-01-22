using Newtonsoft.Json.Linq;
using TrainingPlanParser.Services.Enrichment;
using TrainingPlanParser.Services.Evaluation.Core;

namespace TrainingPlanParser.Services.Evaluation.RuleBased
{
    public class RuleBasedEvaluationResult : EvaluationResult
    {
        public ExpectedStructureData ExpectedData { get; set; } = new();
        public JObject ActualJson { get; set; } = new();
        public List<MissingFieldRequest> MissingFields { get; set; } = new();
    }
}
