using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using TrainingPlanParser.Services.Enrichment;
using TrainingPlanParser.Services.Enrichment.Models;
using TrainingPlanParser.Services.Evaluation.Core;
using TrainingPlanParser.Services.Evaluation.RuleBased;
using TrainingPlanParser.Services.Pipeline;
using TrainingPlanParser.Services.Enrichment.Extraction;

public class TrainingPlanEnrichmentProcessor
{
    private readonly IEnrichmentLlmService _llm;
    private readonly RuleBasedEvaluator _ruleEvaluator;
    private readonly TrainingPlanProcessingPipeline _pipeline;
    private readonly ILogger<TrainingPlanEnrichmentProcessor> _logger;
    private readonly MultiPassEnrichmentOrchestrator _orchestrator;
    public TrainingPlanEnrichmentProcessor(
        IEnrichmentLlmService llm,
        RuleBasedEvaluator ruleEvaluator,
        TrainingPlanProcessingPipeline pipeline,
        MultiPassEnrichmentOrchestrator orchestrator,
        ILogger<TrainingPlanEnrichmentProcessor> logger)
    {
        _llm = llm;
        _ruleEvaluator = ruleEvaluator;
        _pipeline = pipeline;
        _orchestrator = orchestrator;
        _logger = logger;
    }


    public async Task<EnrichmentResult> EnrichAndValidateAsync(
        EvaluationContext originalContext)
    {
        var root = ParseLlmJsonOrThrow(originalContext.LlmJson);

        // INITIAL RULE EVALUATION
        var ruleResult = (RuleBasedEvaluationResult)_ruleEvaluator.Evaluate(
            new EvaluationContext
            {
                RawText = originalContext.RawText,
                StructuredText = originalContext.StructuredText,
                LlmJson = root.ToString()
            });

        // BUILD UNIQUE ENRICH TARGETS (NO DUPLICATES)
        var targets = new Dictionary<string, MissingFieldRequest>();

        foreach (var m in ruleResult.MissingFields)
            targets[m.FieldName] = m;

        foreach (var warning in ruleResult.Warnings)
        {
            var match = Regex.Match(warning, "'(.+?)'");
            if (!match.Success) continue;

            var field = match.Groups[1].Value;

            if (targets.ContainsKey(field)) continue;

            targets[field] = new MissingFieldRequest
            {
                Level = "Root",
                Scope = "ROOT",
                FieldName = field,
                JsonPath = "$",
                ContextSummary = warning,
                IsCritical = false
            };
        }

        if (!targets.Any())
        {
            _logger.LogInformation("[ENRICH] No enrichable fields found.");
        }

        // ENRICHMENT LOOP — GUARANTEED ONE PASS
        var enrichedThisRun = new HashSet<string>();
        var enrichedFields = new List<EnrichedFieldResult>();


        foreach (var missing in targets.Values)
        {
            if (enrichedThisRun.Contains(missing.FieldName))
            {
                _logger.LogInformation("[ENRICH][SKIP] Already enriched {Field}", missing.FieldName);
                continue;
            }

            if (IsAlreadyFilled(root, missing))
            {
                _logger.LogInformation(
                    "[ENRICH][SKIP] {Field} already populated in JSON",
                    missing.FieldName);
                continue;
            }

            _logger.LogInformation("==============================================");
            _logger.LogInformation("[ENRICH] Field: {Field}", missing.FieldName);

            var prompt = EnrichmentPromptBuilder.Build(missing, root);

            _logger.LogInformation("----------- PROMPT SENT TO LLM -----------\n{Prompt}", prompt);

            string generated;
            try
            {
                _logger.LogInformation("[LLM] Calling model...");
                generated = await _llm.GenerateAsync(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LLM ERROR] Call failed for {Field}", missing.FieldName);
                continue;
            }

            _logger.LogInformation(
                "----------- LLM OUTPUT -----------\nField: {Field}\nLength: {Len}\n{Output}",
                missing.FieldName,
                generated?.Length ?? 0,
                generated
            );

            if (string.IsNullOrWhiteSpace(generated))
            {
                _logger.LogWarning("[ENRICH] Empty output for {Field}", missing.FieldName);
                continue;
            }
            // CAPTURE LLM OUTPUT FOR UI
            object generatedValue;

            if (missing.FieldName == "selfAssessmentChecklist")
            {
                generatedValue = generated
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().TrimStart('-', '•'))
                    .Where(s => s.Length > 10)
                    .ToList();
            }
            else
            {
                generatedValue = generated.Trim();
            }

            enrichedFields.Add(new EnrichedFieldResult
            {
                FieldName = missing.FieldName,
                GeneratedValue = generatedValue,
                IsCritical = missing.IsCritical,
                ContextSummary = missing.ContextSummary
            });
            ApplySafely(root, missing, generated);

            enrichedThisRun.Add(missing.FieldName);

            _logger.LogInformation("[ENRICH][APPLIED] {Field}", missing.FieldName);
        }
        var lessonContexts =
            TrainingPlanContextExtractor.ExtractLessons(root);

        var weekContext =
            TrainingPlanContextExtractor.ExtractWeek(root);

        // ---------- LESSON RESOURCES (ONE CALL) ----------
        var resourcesOutput = await _orchestrator.RunAsync(
            new EnrichmentPassRequest
            {
                PassType = EnrichmentPassType.LessonResources,
                Lessons = lessonContexts,
                CurrentPlanJson = root
            });

        JsonMergeHelper.MergeLessonArrays(
            root,
            "resources",
            resourcesOutput
        );

        // ---------- LESSON ASSESSMENTS (ONE CALL) ----------
        var assessmentsOutput = await _orchestrator.RunAsync(
            new EnrichmentPassRequest
            {
                PassType = EnrichmentPassType.LessonAssessments,
                Lessons = lessonContexts,
                CurrentPlanJson = root
            });

        JsonMergeHelper.MergeLessonArrays(
            root,
            "assessments",
            assessmentsOutput
        );

        // ---------- WEEKLY ASSIGNMENT (ONE CALL) ----------
        var assignmentOutput = await _orchestrator.RunAsync(
            new EnrichmentPassRequest
            {
                PassType = EnrichmentPassType.WeeklyAssignment,
                Week = weekContext,
                CurrentPlanJson = root
            });

        JsonMergeHelper.MergeWeekArray(
            root,
            "assignments",
            assignmentOutput
        );
        // FINAL VALIDATION
        var enrichedJson = root.ToString();

        var finalPipelineResult = _pipeline.ExecuteFromJson(
            new EvaluationContext
            {
                RawText = originalContext.RawText,
                StructuredText = originalContext.StructuredText,
                LlmJson = enrichedJson
            });

        return new EnrichmentResult
        {
            EnrichedJson = enrichedJson,
            Evaluation = finalPipelineResult,
            EnrichedFields = enrichedFields
        };


    }
    private static JObject ParseLlmJsonOrThrow(object? llmJson)
    {
        return llmJson switch
        {
            JObject o => o,
            string s when !string.IsNullOrWhiteSpace(s) => JObject.Parse(s),
            JsonElement e => JObject.Parse(e.GetRawText()),
            _ => throw new InvalidOperationException(
                $"Unsupported LlmJson type: {llmJson?.GetType().FullName}")
        };
    }
    private static bool IsAlreadyFilled(JObject root, MissingFieldRequest missing)
    {
        if (missing.Level != "Root") return false;

        var token = root[missing.FieldName];
        if (token == null) return false;

        if (token.Type == JTokenType.Array)
            return token.Any();

        return !string.IsNullOrWhiteSpace(token.ToString());
    }
    private static void ApplySafely(
        JObject root,
        MissingFieldRequest missing,
        string generated)
    {
        var value = generated.Trim();


        if (missing.Level == "Root")
        {
            if (missing.FieldName == "selfAssessmentChecklist")
            {
                var items = value
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().TrimStart('-', '•'))
                    .Where(s => s.Length > 10)
                    .ToList();

                root[missing.FieldName] = new JArray(items);
            }
            else
            {
                root[missing.FieldName] = value;
            }

            return;
        }

        foreach (var obj in root.Descendants().OfType<JObject>())
        {
            if (obj.ContainsKey(missing.FieldName)) continue;
            if (IsStructuralField(missing.FieldName)) continue;

            obj[missing.FieldName] = value;
        }
    }

    private static bool IsStructuralField(string field)
    {
        return field is
            "weeks" or
            "modules" or
            "lessons" or
            "assessments" or
            "resources";
    }
}
