using TrainingPlanParser.Services.Enrichment.Models;

namespace TrainingPlanParser.Services.Enrichment.Prompting
{
    public static class MultiPassPromptBuilder
    {
        public static string Build(EnrichmentPassRequest request)
        {
            return request.PassType switch
            {
                EnrichmentPassType.LessonResources =>
                    BuildLessonResourcesPrompt(request.Lessons),

                EnrichmentPassType.LessonAssessments =>
                    BuildLessonAssessmentsPrompt(request.Lessons),

                EnrichmentPassType.WeeklyAssignment =>
                    BuildWeeklyAssignmentPrompt(request.Week!),

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        // =====================================================
        // LESSON RESOURCES — BATCH (ONE CALL)
        // =====================================================
        private static string BuildLessonResourcesPrompt(List<LessonContext> lessons)
        {
            return $@"
Generate EXACTLY ONE learning resource per lesson.

STRICT RULES:
- Output ONLY valid JSON
- Output MUST be a JSON ARRAY
- Array length MUST be EXACTLY {lessons.Count}
- Order MUST match lesson order
- Each item is a SINGLE resource object
- No explanations, no markdown, no text outside JSON

Resource schema:
{{
  ""title"": """",
  ""url"": """",
  ""resourceType"": """",
  ""description"": """"
}}

Lessons (ORDER MATTERS):
{FormatLessonsWithIndex(lessons)}

OUTPUT JSON:
[
  {{ ""title"": """", ""url"": """", ""resourceType"": """", ""description"": """" }}
]
";
        }

        // =====================================================
        // 🔥 LESSON ASSESSMENTS — FULLY EVALUATABLE (ONE PER LESSON)
        // =====================================================
        // =====================================================
        // 🔥 LESSON ASSESSMENTS — FULLY EVALUATABLE (SAFE SIZE)
        // =====================================================
        private static string BuildLessonAssessmentsPrompt(List<LessonContext> lessons)
        {
            return $@"
Generate EXACTLY ONE COMPLETE assessment per lesson.

STRICT RULES:
- Output ONLY valid JSON
- Output MUST be a JSON ARRAY
- Array length MUST be EXACTLY {lessons.Count}
- Order MUST match lesson order
- Each item is a SINGLE assessment object
- No explanations
- No markdown
- No text outside JSON

Assessment schema:
{{
  ""title"": """",
  ""assessmentType"": ""Quiz | Descriptive | CodingChallenge"",
  ""difficulty"": 1,
  ""estimatedDurationMinutes"": 15,
  ""learningObjectives"": """",
  ""description"": """",
  ""questions"": [
    {{
      ""question"": """",
      ""questionType"": ""MCQ | Descriptive | Coding"",
      ""options"": ""JSON string or empty"",
      ""correctAnswer"": """",
      ""explanation"": """",
      ""marks"": 0,
      ""evaluationStrategy"": """"
    }}
  ]
}}

QUESTION RULES:
- Each assessment MUST have EXACTLY 1 question
- Keep ALL text concise
- MCQ questions MUST include:
  - options (JSON array string)
  - correctAnswer
- Descriptive questions MUST include:
  - evaluationStrategy
- Coding questions MUST include:
  - evaluationStrategy
- Use empty strings """" where not applicable
- Keep structure CONSISTENT across all lessons

Lessons (ORDER MATTERS):
{FormatLessonsWithIndex(lessons)}

OUTPUT JSON:
[
  {{
    ""title"": """",
    ""assessmentType"": ""Quiz"",
    ""difficulty"": 1,
    ""estimatedDurationMinutes"": 15,
    ""learningObjectives"": """",
    ""description"": """",
    ""questions"": [
      {{
        ""question"": """",
        ""questionType"": ""MCQ"",
        ""options"": ""[\""A\"", \""B\"", \""C\"", \""D\""]"",
        ""correctAnswer"": ""A"",
        ""explanation"": """",
        ""marks"": 1,
        ""evaluationStrategy"": ""AUTO""
      }}
    ]
  }}
]
";
        }


        // =====================================================
        // WEEKLY ASSIGNMENT — SINGLE CALL
        // =====================================================
        private static string BuildWeeklyAssignmentPrompt(WeekContext week)
        {
            return $@"
You are generating ONE weekly assignment.

STRICT RULES:
- Output ONLY valid JSON
- Output MUST be a JSON ARRAY
- Array length MUST be 1
- No explanations
- No markdown

Assignment schema:
{{
  ""title"": """",
  ""description"": """",
  ""estimatedDurationMinutes"": 0,
  ""metadata"": """"
}}

Week:
{week.Title}

Lessons covered:
{FormatLessonsWithIndex(week.Lessons)}

OUTPUT JSON:
[
  {{ ""title"": """", ""description"": """", ""estimatedDurationMinutes"": 0, ""metadata"": """" }}
]
";
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private static string FormatLessonsWithIndex(List<LessonContext> lessons)
            => string.Join(
                "\n",
                lessons.Select((l, i) =>
                    $"{i + 1}. {l.Title} — {l.Summary}")
            );
    }
}
