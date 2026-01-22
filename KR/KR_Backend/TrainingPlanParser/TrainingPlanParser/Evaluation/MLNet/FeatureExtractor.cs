using TrainingPlanParser.Services.Evaluation.RuleBased;

namespace TrainingPlanParser.Services.Evaluation.MLNet
{
    public static class FeatureExtractor
    {
        public static ModelInput Extract(RuleBasedEvaluationResult r)
        {
            return new ModelInput
            {
                CriticalErrors = r.Errors.Count(e => e.Contains("missing 'weeks'")),
                StructuralErrors = r.Errors.Count(e => e.Contains("Week") || e.Contains("Module count")),
                MissingFields = r.Errors.Count(e => e.Contains("missing required field")),
                EmptyContent = r.Warnings.Count(w => w.Contains("empty or missing")),
                Hallucinations = r.Warnings.Count(w => w.Contains("Unexpected field")),
                UncertaintySignals = r.Warnings.Count(w => w.Contains("uncertain")),
                Warnings = r.Warnings.Count,
                RuleScore = (float)r.Score
            };
        }
    }
}
