using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace TrainingPlanParser.Services
{
    public class GroqLlmService : ILlmService
    {
        private readonly HttpClient _http;

        public GroqLlmService(string apiKey)
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<string> GenerateStructuredJson(string parsedText)
        {
            const string url = "https://api.groq.com/openai/v1/chat/completions";
            const string schema = @"
{
  ""title"": """",
  ""description"": """",
  ""overview"": """",
  ""objectives"": """",
  ""durationWeeks"": 0,
  ""totalDays"": 0,
  ""prerequisites"": [],
  ""technicalRequirements"": [],
  ""selfAssessmentChecklist"": [],

  ""weeks"": [
    {
      ""weekNumber"": 1,
      ""description"": """",
      ""prerequisites"": [],
      ""learningObjectives"": [],
      ""metadata"": """",

      ""modules"": [
        {
          ""moduleName"": """",
          ""description"": """",
          ""overview"": """",
          ""orderNo"": 1,
          ""durationDays"": 0,
          ""prerequisites"": [],
          ""metadata"": """",

          ""lessons"": [
            {
              ""title"": """",
              ""content"": """",
              ""lessonType"": """",
              ""orderIndex"": 1,
              ""overview"": """",
              ""prerequisites"": [],
              ""durationMinutes"": 0,

              ""resources"": [
                {
                  ""title"": """",
                  ""url"": """",
                  ""resourceType"": """",
                  ""description"": """",
                  ""metadata"": """"
                }
              ],

              ""assessments"": [
                {
                  ""title"": """",
                  ""assessmentType"": """",
                  ""difficulty"": 1,
                  ""estimatedDurationMinutes"": 0,
                  ""learningObjectives"": """",
                  ""description"": """",
                  ""metadata"": """"
                }
              ]
            }
          ]
        }
      ],

      ""assignments"": [
        {
          ""title"": """",
          ""description"": """",
          ""estimatedDurationMinutes"": 0
        }
      ]
    }
  ]
}
";
            // SYSTEM PROMPT — HARD CONSTRAINTS =
            string systemPrompt = @"
You convert parsed training plans into VALID JSON.

CRITICAL RULES (MUST FOLLOW):
- Output ONLY valid JSON
- JSON MUST match the schema EXACTLY
- DO NOT add new fields
- DO NOT rename fields
- DO NOT omit required fields
- DO NOT infer missing data
- If data is missing:
  - Use empty string for strings
  - Use empty array for arrays
  - Use 0 for numbers

The output MUST be directly deserializable into TrainingPlanIngestionDto.

===================================================
FOLLOW THIS EXACT JSON SCHEMA:
" + schema + @"

===================================================
PARSER MARKERS (SOURCE OF TRUTH)
===================================================

1. TABLE STRUCTURE
[TABLE-START] ... [TABLE-END] defines ONE WEEK.

2. WEEK IDENTIFICATION
[CELL index=""0""] = ""Week""
[CELL index=""1""] = ""Week Number""
[CELL index=""2""] = numeric week number

3. MODULE IDENTIFICATION
[CELL index=""0""] starts with ""Module""

4. LESSON IDENTIFICATION
[CELL index=""0""] contains ""Module X - Lesson Y""

5. RESOURCES
[CELL index=""0""] = ""Resources""

6. ASSESSMENTS
[CELL index=""0""] = ""Assessment""

7. ASSIGNMENTS
[CELL index=""0""] = ""Assignment""

8. STRICT HIERARCHY
COURSE → WEEK → MODULE → LESSON
No skipping levels. No guessing. No reordering.
";

            var payload = new
            {
                model = "llama-3.3-70b-versatile",
                temperature = 0.1,
                response_format = new { type = "json_object" },
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = parsedText }
                }
            };

            var body = JsonConvert.SerializeObject(payload);

            using var response = await _http.PostAsync(
                url,
                new StringContent(body, Encoding.UTF8, "application/json"));

            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseBody);

            if (result == null)
                throw new Exception("Groq returned NULL response.");
            // ERROR HANDLING 
            if (result.error != null)
            {
                var message = (string)result.error.message;

                if (message.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        error = "RATE_LIMIT_REACHED",
                        message,
                        partialOutput = result?.choices?[0]?.message?.content?.ToString() ?? ""
                    }, Formatting.Indented);
                }

                return JsonConvert.SerializeObject(new
                {
                    error = "API_ERROR",
                    message
                }, Formatting.Indented);
            }
            return result.choices[0].message.content.ToString();
        }
    }
}
