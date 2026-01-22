using Microsoft.Extensions.Logging;
using TrainingPlanParser.Services;
using TrainingPlanParser.Services.Evaluation.Core;
using TrainingPlanParser.Services.Evaluation.Hybrid;
using TrainingPlanParser.Services.Evaluation.MLNet;
using TrainingPlanParser.Services.Evaluation.RuleBased;
using TrainingPlanParser.Services.Pipeline.Models;

namespace TrainingPlanParser.Services.Pipeline
{
    public class TrainingPlanProcessingPipeline
    {
        private readonly DocxParser _parser;
        private readonly ILlmService _llm;
        private readonly ILogger<TrainingPlanProcessingPipeline> _logger;

        public TrainingPlanProcessingPipeline(
            DocxParser parser,
            ILlmService llm,
            ILogger<TrainingPlanProcessingPipeline> logger)
        {
            _parser = parser;
            _llm = llm;
            _logger = logger;
        }

        public async Task<TrainingPlanPipelineResult> ExecuteAsync(string docxPath)
        {
            // 1. PARSE DOCUMENT
            var (rawText, structuredText) = _parser.ExtractBoth(docxPath);

            // 2. CALL LLM (STRUCTURE ONLY)
            var llmJson = await _llm.GenerateStructuredJson(structuredText);

            _logger.LogInformation("===== LLM STRUCTURED JSON OUTPUT =====");
            _logger.LogInformation(llmJson);
            _logger.LogInformation("=====================================");

            var ctx = new EvaluationContext
            {
                RawText = rawText,
                StructuredText = structuredText,
                LlmJson = llmJson
            };

            // 3. INITIALIZE EVALUATORS
            var ruleEvaluator = new RuleBasedEvaluator();
            var mlEvaluator = new MLNetEvaluator();
            var hybridEvaluator = new HybridEvaluator(ruleEvaluator, mlEvaluator);

            // 4. RUN RULE-BASED VALIDATION
            var ruleResult =
                (RuleBasedEvaluationResult)ruleEvaluator.Evaluate(ctx);

            // 5. RUN ML.NET (ALWAYS — FOR DIAGNOSTICS)
            var mlResult = mlEvaluator.Evaluate(ctx);

            // 6. RUN HYBRID (ALWAYS — FOR EXPLAINABILITY)
            var hybridResult = hybridEvaluator.Evaluate(ctx);
            // 7. FINAL DECISION
            // Rule-based is a HARD gate for approval,
            // but ML + Hybrid metrics are still exposed
            bool finalIsValid =
                ruleResult.IsValid && hybridResult.IsValid;

            decimal finalScore =
                finalIsValid
                    ? (decimal)hybridResult.Score
                    : (decimal)ruleResult.Score;

            // 8. RETURN FULL PIPELINE RESULT
            return new TrainingPlanPipelineResult
            {
                RawText = rawText,
                StructuredText = structuredText,
                LlmJson = llmJson,

                IsValid = finalIsValid,
                FinalScore = finalScore,

                RuleBased = ruleResult,
                ML = mlResult,
                Hybrid = hybridResult
            };
        }
        public TrainingPlanPipelineResult ExecuteFromJson(EvaluationContext ctx)
        {
            var ruleEvaluator = new RuleBasedEvaluator();
            var mlEvaluator = new MLNetEvaluator();
            var hybridEvaluator = new HybridEvaluator(ruleEvaluator, mlEvaluator);

            var ruleResult =
                (RuleBasedEvaluationResult)ruleEvaluator.Evaluate(ctx);

            var mlResult = mlEvaluator.Evaluate(ctx);
            var hybridResult = hybridEvaluator.Evaluate(ctx);

            bool finalIsValid =
                ruleResult.IsValid && hybridResult.IsValid;

            decimal finalScore =
                finalIsValid
                    ? (decimal)hybridResult.Score
                    : (decimal)ruleResult.Score;

            return new TrainingPlanPipelineResult
            {
                RawText = ctx.RawText,
                StructuredText = ctx.StructuredText,
                LlmJson = ctx.LlmJson,

                IsValid = finalIsValid,
                FinalScore = finalScore,

                RuleBased = ruleResult,
                ML = mlResult,
                Hybrid = hybridResult
            };
        }

    }
}
