using TrainingPlanParser.Services.Evaluation.Core;

namespace TrainingPlanParser.Services.Evaluation.Hybrid
{
    public class HybridEvaluator : IEvaluationStrategy
    {
        public string Name => "Hybrid (Rules + ML)";

        private readonly IEvaluationStrategy _rules;
        private readonly IEvaluationStrategy _ml;

        public HybridEvaluator(
            IEvaluationStrategy rules,
            IEvaluationStrategy ml)
        {
            _rules = rules;
            _ml = ml;
        }
        public EvaluationResult Evaluate(EvaluationContext ctx)
        {
            var ruleResult = _rules.Evaluate(ctx);
            var mlResult = _ml.Evaluate(ctx);

            const double RULE_WEIGHT = 0.7;
            const double ML_WEIGHT = 0.3;
            bool ruleRejected = !ruleResult.IsValid;

            double combinedScore = ruleRejected
                ? ruleResult.Score
                : ruleResult.Score * RULE_WEIGHT + mlResult.Score * ML_WEIGHT;

            bool finalDecision =
                !ruleRejected &&
                combinedScore >= 60 &&
                mlResult.IsValid;
            var finalResult = new EvaluationResult
            {
                IsValid = finalDecision,
                Score = Math.Round(combinedScore, 2)
            };

            finalResult.Errors.AddRange(ruleResult.Errors);
            finalResult.Warnings.AddRange(ruleResult.Warnings);

            finalResult.Metrics["RuleScore"] = ruleResult.Score;
            finalResult.Metrics["MLScore"] = mlResult.Score;
            finalResult.Metrics["FinalScore"] = finalResult.Score;

            finalResult.Metrics["RuleWeight"] = RULE_WEIGHT;
            finalResult.Metrics["MLWeight"] = ML_WEIGHT;

            finalResult.Metrics["RejectedByRules"] = ruleRejected ? 1 : 0;
            finalResult.Metrics["DecisionSource_Rules"] = ruleRejected ? 1 : 0;
            finalResult.Metrics["DecisionSource_ML"] = ruleRejected ? 0 : 1;

            finalResult.Metrics["HybridAccepted"] = finalDecision ? 1 : 0;

            foreach (var kv in mlResult.Metrics)
            {
                finalResult.Metrics[$"ML_{kv.Key}"] = kv.Value;
            }

            return finalResult;
        }

    }
}
