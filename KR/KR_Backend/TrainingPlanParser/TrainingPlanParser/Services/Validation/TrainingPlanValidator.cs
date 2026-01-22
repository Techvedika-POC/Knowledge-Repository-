using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace TrainingPlanParser.Services.Validation
{
    public class TrainingPlanValidator
    {
        public ValidationResult Validate(string structuredText, string llmJson)
        {
            var result = new ValidationResult();

            JObject root;
            try
            {
                root = JObject.Parse(llmJson);
            }
            catch
            {
                result.Errors.Add("Invalid JSON format.");
                FinalizeScore(result);
                return result;
            }

            // WEEK COUNT CHECK
            int parsedWeeks =
                Regex.Matches(structuredText, @"\[WEEK name=").Count;

            int jsonWeeks =
                (root["weeks"] as JArray)?.Count ?? 0;

            if (parsedWeeks != jsonWeeks)
            {
                result.Errors.Add(
                    $"Week count mismatch: Parsed={parsedWeeks}, JSON={jsonWeeks}");
            }

            // STRUCTURE CHECK
            var weeks = root["weeks"] as JArray;

            if (weeks == null || weeks.Count == 0)
            {
                result.Errors.Add("No weeks found in JSON.");
                return result;
            }
            foreach (var week in weeks ?? new JArray())
            {
                if (week["modules"] == null)
                    result.Errors.Add("Week missing modules.");

                if (week["assignments"] == null)
                    result.Warnings.Add("Week missing assignments.");
            }

            FinalizeScore(result);
            return result;
        }

        private void FinalizeScore(ValidationResult result)
        {
            int score = 100;
            score -= result.Errors.Count * 15;
            score -= result.Warnings.Count * 5;

            result.AccuracyScore = Math.Max(0, score);
            result.IsValid = result.Errors.Count == 0;
        }
    }
}
