using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TrainingPlanParser.Services.Enrichment.Models;
using TrainingPlanParser.Services.Enrichment.Prompting;

namespace TrainingPlanParser.Services.Enrichment
{
    public class MultiPassEnrichmentOrchestrator
    {
        private readonly IEnrichmentLlmService _llm;
        private readonly ILogger<MultiPassEnrichmentOrchestrator> _logger;

        public MultiPassEnrichmentOrchestrator(
            IEnrichmentLlmService llm,
            ILogger<MultiPassEnrichmentOrchestrator> logger)
        {
            _llm = llm;
            _logger = logger;
        }

        public async Task<string> RunAsync(EnrichmentPassRequest request)
        {
            var basePrompt = MultiPassPromptBuilder.Build(request);

            var prompt = $@"
{basePrompt}

CRITICAL OUTPUT RULES:
- Return ONLY valid JSON
- No explanations
- No markdown
- No labels
- JSON must be directly parsable
";

            _logger.LogInformation("[ENRICH][PASS] {Pass}", request.PassType);

            string output;

            try
            {
                output = await _llm.GenerateBatchAsync(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ENRICH][LLM ERROR][{Pass}]", request.PassType);
                return "[]";
            }

            _logger.LogInformation(
                "[ENRICH][RAW OUTPUT][{Pass}]\n{Output}",
                request.PassType,
                output);

            var recoveredJson = RecoverJsonArray(output);

            if (recoveredJson == null)
            {
                _logger.LogWarning(
                    "[ENRICH][UNRECOVERABLE JSON][{Pass}] Output discarded",
                    request.PassType);

                return "[]";
            }

            return recoveredJson;
        }
        private static string? RecoverJsonArray(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            try
            {
                JToken.Parse(text);
                return text;
            }
            catch
            {
                // ignored
            }

            var lastClosingBracket = text.LastIndexOf(']');
            if (lastClosingBracket <= 0)
                return null;

            var trimmed = text.Substring(0, lastClosingBracket + 1);

            try
            {
                JToken.Parse(trimmed);
                return trimmed;
            }
            catch
            {
                return null;
            }
        }
    }
}
