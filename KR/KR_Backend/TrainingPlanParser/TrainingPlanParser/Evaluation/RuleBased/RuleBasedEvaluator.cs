using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using TrainingPlanParser.Services.Evaluation.Core;
using TrainingPlanParser.Services.Enrichment;

namespace TrainingPlanParser.Services.Evaluation.RuleBased
{
    public class RuleBasedEvaluator : IEvaluationStrategy
    {
        public string Name => "Rule-Based Content & Structure Validation";

        public EvaluationResult Evaluate(EvaluationContext ctx)
        {
            var result = new RuleBasedEvaluationResult();

            JObject root;
            try
            {
                root = JObject.Parse(ctx.LlmJson);
                result.ActualJson = root;
            }
            catch
            {
                result.Errors.Add("Invalid JSON format.");
                FinalizeResult(result);
                return result;
            }

            // =====================================================
            // 1. EXPECTED STRUCTURE
            // =====================================================
            var expectedWeeks = ExtractWeeks(ctx.StructuredText);
            var expectedModules = ExtractModulesPerWeek(ctx.StructuredText);

            result.ExpectedData.Weeks = expectedWeeks;
            result.ExpectedData.ModulesPerWeek = expectedModules;

            // =====================================================
            // 2. ROOT VALIDATION
            // =====================================================
            var weeks = root["weeks"] as JArray;
            if (weeks == null)
            {
                AddMissing(result, "Root", "ROOT", "weeks", true);
                FinalizeResult(result);
                return result;
            }

            CheckRequired(root, "title", "ROOT", "Root", result, true);
            CheckRequired(root, "description", "ROOT", "Root", result);
            CheckRequired(root, "overview", "ROOT", "Root", result);
            CheckRequired(root, "objectives", "ROOT", "Root", result);
            CheckRequired(root, "prerequisites", "ROOT", "Root", result);
            CheckRequired(root, "technicalRequirements", "ROOT", "Root", result);
            CheckRequired(root, "selfAssessmentChecklist", "ROOT", "Root", result);

            // =====================================================
            // 3. WEEK / MODULE / LESSON VALIDATION
            // =====================================================
            for (int i = 0; i < Math.Min(weeks.Count, expectedWeeks.Count); i++)
            {
                var expectedWeek = expectedWeeks[i];
                var jsonWeek = (JObject)weeks[i];
                if (!jsonWeek.ContainsKey("title") && !jsonWeek.ContainsKey("weekNumber"))
                {
                    AddMissing(result, "Week", expectedWeek, "title/weekNumber", false);
                }

                CheckRequired(jsonWeek, "description", expectedWeek, "Week", result);
                CheckRequired(jsonWeek, "learningObjectives", expectedWeek, "Week", result);
                CheckRequired(jsonWeek, "prerequisites", expectedWeek, "Week", result);

                var modules = jsonWeek["modules"] as JArray ?? new JArray();

                foreach (var moduleToken in modules.OfType<JObject>())
                {
                    var moduleName = moduleToken["moduleName"]?.ToString() ?? "Unknown Module";
                    var moduleScope = $"Module '{moduleName}'";

                    CheckRequired(moduleToken, "description", moduleScope, "Module", result);
                    CheckRequired(moduleToken, "overview", moduleScope, "Module", result);
                    CheckRequired(moduleToken, "prerequisites", moduleScope, "Module", result);
                    CheckRequired(moduleToken, "lessons", moduleScope, "Module", result, true);
                    CheckRequired(moduleToken, "resources", moduleScope, "Module", result);
                    CheckRequired(moduleToken, "assessments", moduleScope, "Module", result);

                    var lessonsToken = moduleToken["lessons"];

                    if (lessonsToken == null || lessonsToken.Type != JTokenType.Array)
                    {
                        AddMissing(result, "Module", moduleScope, "lessons", true);
                        continue;
                    }

                    foreach (var lesson in lessonsToken.OfType<JObject>())
                    {
                        var lessonTitle = lesson["title"]?.ToString() ?? "Unnamed Lesson";
                        var lessonScope = $"Lesson '{lessonTitle}'";

                        CheckRequired(lesson, "title", lessonScope, "Lesson", result, true);
                        CheckRequired(lesson, "content", lessonScope, "Lesson", result);
                        CheckRequired(lesson, "lessonType", lessonScope, "Lesson", result);
                        CheckRequired(lesson, "durationMinutes", lessonScope, "Lesson", result);
                    }

                }

                if (jsonWeek["assignments"] == null)
                    AddMissing(result, "Week", expectedWeek, "assignments", false);
            }

            FinalizeResult(result);
            return result;
        }

        private void CheckRequired(
            JObject obj,
            string field,
            string scope,
            string level,
            RuleBasedEvaluationResult result,
            bool critical = false)
        {
            if (!obj.TryGetValue(field, out var token) ||
                token == null ||
                (token.Type == JTokenType.String && string.IsNullOrWhiteSpace(token.ToString())) ||
                (token is JArray arr && !arr.Any()))
            {
                AddMissing(result, level, scope, field, critical);
            }
        }

        private void AddMissing(
            RuleBasedEvaluationResult result,
            string level,
            string scope,
            string field,
            bool critical)
        {
            var message = $"{scope} missing or empty required field '{field}'";

            if (critical)
                result.Errors.Add(message);
            else
                result.Warnings.Add(message);

            result.MissingFields.Add(new MissingFieldRequest
            {
                Level = level,
                Scope = scope,
                FieldName = field,
                IsCritical = critical,
                JsonPath = BuildJsonPath(level, scope, field),
                ContextSummary = $"Generate '{field}' for {level} ({scope}) using surrounding content."
            });
        }

        private string BuildJsonPath(string level, string scope, string field)
        {
            return level switch
            {
                "Root" => $"$.{field}",
                "Week" => $"$.weeks[?(@.title=='{scope}')].{field}",
                "Module" => $"$.weeks[*].modules[?(@.moduleName=='{scope.Replace("Module '", "").Replace("'", "")}')].{field}",
                "Lesson" => $"$.weeks[*].modules[*].lessons[?(@.title=='{scope.Replace("Lesson '", "").Replace("'", "")}')].{field}",
                _ => $"$.{field}"
            };
        }

        private void FinalizeResult(RuleBasedEvaluationResult result)
        {
            result.Score = Math.Max(
                0,
                100 - (result.Errors.Count * 10) - (result.Warnings.Count * 3));
            result.IsValid = result.MissingFields.Count == 0;
        }


        private List<string> ExtractWeeks(string structuredText)
        {
            return Regex.Matches(structuredText, @"\[WEEK\s+name=""([^""]+)""\]")
                .Select(m => m.Groups[1].Value.Trim())
                .Distinct()
                .ToList();
        }

        private Dictionary<string, List<string>> ExtractModulesPerWeek(string structuredText)
        {
            var result = new Dictionary<string, List<string>>();
            string? currentWeek = null;

            foreach (var line in structuredText.Split('\n'))
            {
                var weekMatch = Regex.Match(line, @"\[WEEK\s+name=""([^""]+)""\]");
                if (weekMatch.Success)
                {
                    currentWeek = weekMatch.Groups[1].Value.Trim();
                    result[currentWeek] = new();
                    continue;
                }

                if (currentWeek != null && line.StartsWith("[MODULE]"))
                    result[currentWeek].Add(line.Replace("[MODULE]", "").Trim());
            }

            return result;
        }
    }
}
