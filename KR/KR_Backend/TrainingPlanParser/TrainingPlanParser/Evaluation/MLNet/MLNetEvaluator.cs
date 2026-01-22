using Microsoft.ML;
using TrainingPlanParser.Services.Evaluation.Core;
using TrainingPlanParser.Services.Evaluation.RuleBased;

namespace TrainingPlanParser.Services.Evaluation.MLNet
{
    public class MLNetEvaluator : IEvaluationStrategy
    {
        private readonly ITransformer _model;
        private readonly MLContext _mlContext;

        public string Name => "ML.NET Quality Classifier";

        public MLNetEvaluator()
        {
            _model = MLNetModelBuilder.LoadModel(out _mlContext);
        }

        public EvaluationResult Evaluate(EvaluationContext ctx)
        {
            var rules = new RuleBasedEvaluator();
            var ruleResult = (RuleBasedEvaluationResult)rules.Evaluate(ctx);

            var features = FeatureExtractor.Extract(ruleResult);

            var engine = _mlContext.Model
                .CreatePredictionEngine<ModelInput, ModelOutput>(_model);

            var output = engine.Predict(features);

            return new EvaluationResult
            {
                IsValid = output.Prediction,
                Score = Math.Round(output.Probability * 100, 2),
                Metrics =
        {
            ["ML_Confidence"] = output.Probability,
            ["CriticalErrors"] = features.CriticalErrors,
            ["StructuralErrors"] = features.StructuralErrors,
            ["MissingFields"] = features.MissingFields,
            ["EmptyContent"] = features.EmptyContent,
            ["Hallucinations"] = features.Hallucinations,
            ["UncertaintySignals"] = features.UncertaintySignals
        }
            };
        }

    }
}
