using TrainingPlanParser.Services.Evaluation.Core;

namespace TrainingPlanParser.Services.Evaluation
{
    public class EvaluationRunner
    {
        private readonly List<IEvaluationStrategy> _strategies;

        public EvaluationRunner(IEnumerable<IEvaluationStrategy> strategies)
        {
            _strategies = strategies.ToList();
        }

        public Dictionary<string, EvaluationResult> Run(EvaluationContext ctx)
        {
            return _strategies.ToDictionary(
                s => s.Name,
                s => s.Evaluate(ctx)
            );
        }
    }
}
