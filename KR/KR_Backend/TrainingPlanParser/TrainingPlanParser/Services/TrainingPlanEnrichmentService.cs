using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TrainingPlanParser.Services;
using TrainingPlanParser.Services.Evaluation.RuleBased;

namespace TrainingPlanParser.Services.Enrichment
{
    public class TrainingPlanEnrichmentService
    {
        private readonly ILlmService _llm;
        private const int CHUNK_SIZE = 5;

        public TrainingPlanEnrichmentService(ILlmService llm)
        {
            _llm = llm;
        }
        public async Task<JObject> EnrichAsync(
            JObject baseJson,
            List<MissingFieldRequest> missingFields)
        {
            if (missingFields == null || !missingFields.Any())
                return baseJson;

            var enrichedJson = (JObject)baseJson.DeepClone();

            var chunks = missingFields
                .Select((f, i) => new { f, i })
                .GroupBy(x => x.i / CHUNK_SIZE)
                .Select(g => g.Select(x => x.f).ToList())
                .ToList();

            Console.WriteLine(
                $"🧠 Enrichment running in {chunks.Count} batches " +
                $"({missingFields.Count} total fields)");

            int batchNo = 1;

            foreach (var chunk in chunks)
            {
                Console.WriteLine($"➡ Batch {batchNo++}/{chunks.Count}");

                string prompt = BuildBatchPrompt(chunk);

                JObject? patch = await GeneratePatchWithRetry(prompt);

                if (patch == null)
                {
                    Console.WriteLine("❌ Batch failed after retry, skipping");
                    continue;
                }

                foreach (var entry in patch.Properties())
                {
                    ApplyValue(enrichedJson, entry.Name, entry.Value);
                }

                Console.WriteLine($"✔ Applied {patch.Properties().Count()} fields");
            }

            return enrichedJson;
        }

        // LLM CALL + JSON EXTRACTION + RETRY
        private async Task<JObject?> GeneratePatchWithRetry(string prompt)
        {
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                string raw = await _llm.GenerateStructuredJson(prompt);

                var patch = TryExtractJsonObject(raw);
                if (patch != null)
                    return patch;

                Console.WriteLine($"⚠ Invalid JSON (attempt {attempt}), retrying...");
            }

            return null;
        }

        // JSON EXTRACTION (CRITICAL)
        private JObject? TryExtractJsonObject(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            raw = raw.Replace("```json", "")
                     .Replace("```", "")
                     .Trim();

            int start = raw.IndexOf('{');
            int end = raw.LastIndexOf('}');

            if (start < 0 || end <= start)
                return null;

            string json = raw.Substring(start, end - start + 1);

            try
            {
                return JObject.Parse(json);
            }
            catch
            {
                return null;
            }
        }
        // PROMPT BUILDER (PATCH-BASED)
        private string GetExpectedType(string fieldName) =>
            fieldName switch
            {
                "prerequisites" => "array<string>",
                "learningObjectives" => "array<string>",
                "technicalRequirements" => "array<string>",
                "selfAssessmentChecklist" => "array<string>",
                "resources" => "array<object>",
                "assessments" => "array<object>",
                "assignments" => "array<object>",
                "durationWeeks" => "number",
                "totalDays" => "number",
                "durationMinutes" => "number",
                _ => "string"
            };

        private string BuildBatchPrompt(List<MissingFieldRequest> fields)
        {
            var requests = fields.Select(f => new
            {
                f.JsonPath,
                f.Level,
                f.Scope,
                f.FieldName,
                ExpectedType = GetExpectedType(f.FieldName)
            });

            return $@"
You are an expert instructional designer.

Generate content for ALL missing fields below.

==================================================
FIELDS TO GENERATE
==================================================
{JsonConvert.SerializeObject(requests, Formatting.Indented)}

==================================================
STRICT RULES
==================================================
- Output MUST start with '{{' and end with '}}'
- Output MUST be valid JSON
- ROOT object ONLY
- Keys MUST be EXACT JsonPath values
- Values MUST match ExpectedType
- NO explanations
- NO markdown
- NO extra text

==================================================
OUTPUT FORMAT (EXACT)
==================================================
{{
  ""$.overview"": ""Program overview..."",
  ""$.weeks[0].modules[0].description"": ""Module description...""
}}
";
        }
        private void ApplyValue(JObject root, string jsonPath, JToken newValue)
        {
            var token = root.SelectToken(jsonPath);
            if (token == null)
                return;

            if (token.Type == JTokenType.Null ||
                (token.Type == JTokenType.String && string.IsNullOrWhiteSpace(token.ToString())) ||
                (token is JArray arr && !arr.Any()))
            {
                token.Replace(newValue);
            }
        }
    }
}
