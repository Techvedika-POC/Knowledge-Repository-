using Newtonsoft.Json.Linq;
using TrainingPlanParser.Services.Evaluation.RuleBased;

namespace TrainingPlanParser.Services.Enrichment
{
    public static class EnrichmentPromptBuilder
    {
        public static string Build(MissingFieldRequest field, JObject root)
        {
            if (field.FieldName.Equals("assessments", StringComparison.OrdinalIgnoreCase))
            {
                return BuildAssessmentPrompt(field, root);
            }
            return $@"
You are enriching an existing learning plan.

Target Field:
- Level: {field.Level}
- Scope: {field.Scope}
- Name: {field.FieldName}

Context / Reason Missing:
{field.ContextSummary}

Relevant Existing Content:
{ExtractRelevantContent(root, field.JsonPath)}

Instructions:
- Generate meaningful, non-empty content
- Match the tone and depth of the rest of the plan
- Do NOT explain your answer
- Do NOT return empty output

Now generate ONLY the content for '{field.FieldName}':
";
        }

        // ASSESSMENT-SPECIFIC PROMPT 
        private static string BuildAssessmentPrompt(
            MissingFieldRequest field,
            JObject root)
        {
            return $@"
You are generating COMPLETE lesson assessments for a learning plan.

Target:
- Field: assessments
- Level: {field.Level}
- Scope: {field.Scope}

Context:
{field.ContextSummary}

Relevant Lesson Content:
{ExtractRelevantContent(root, field.JsonPath)}

MANDATORY RULES:
- Output MUST be a JSON ARRAY
- Each item MUST be a complete assessment object
- Do NOT explain anything
- Do NOT return empty output
- Do NOT include markdown

Assessment requirements:
Each assessment MUST include:
- title
- assessmentType (Quiz | Descriptive | CodingChallenge)
- difficulty (1 to 5)
- estimatedDurationMinutes
- learningObjectives
- description

Each assessment MUST include a 'questions' array.

Question rules:
- Minimum 3 questions per assessment
- questionType must be one of: MCQ | Descriptive | Coding

For MCQ questions:
- include options (array of strings)
- include correctAnswer

For Descriptive questions:
- include evaluationStrategy
- include sampleAnswer

For Coding questions:
- include problemStatement
- include evaluationStrategy
- include sampleSolution

IMPORTANT:
- Keep structure consistent across all assessments
- Content must be suitable for learner evaluation
- Output ONLY the JSON array

Now generate the assessments:
";
        }

        private static string ExtractRelevantContent(JObject root, string jsonPath)
        {
            try
            {
                var token = root.SelectToken(jsonPath);
                return token?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}

