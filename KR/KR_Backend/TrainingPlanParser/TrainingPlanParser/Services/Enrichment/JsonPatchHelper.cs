using Newtonsoft.Json.Linq;
using TrainingPlanParser.Services.Evaluation.RuleBased;

namespace TrainingPlanParser.Services.Enrichment
{
    public static class JsonPatchHelper
    {
        public static void Apply(
            JObject root,
            MissingFieldRequest field,
            string generatedValue)
        {
            var token = root.SelectToken(field.JsonPath);
            if (token == null) return;

            if (token.Type == JTokenType.Array)
            {
                var arr = (JArray)token;
                if (!string.IsNullOrWhiteSpace(generatedValue))
                    arr.Add(JToken.FromObject(generatedValue));
            }
            else
            {
                token.Replace(JToken.FromObject(generatedValue));
            }
        }
    }
}
