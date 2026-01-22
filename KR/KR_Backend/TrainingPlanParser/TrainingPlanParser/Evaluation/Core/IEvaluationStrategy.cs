namespace TrainingPlanParser.Services.Evaluation.Core
{
    public interface IEvaluationStrategy
    {
        string Name { get; }
        EvaluationResult Evaluate(EvaluationContext context);
    }
}
