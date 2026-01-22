using Newtonsoft.Json.Linq;

namespace TrainingPlanParser.Services.Enrichment
{
    public static class JsonMergeHelper
    {
        public static void MergeLessonArrays(
            JObject root,
            string propertyName,
            string llmJson)
        {
            if (!TryParseArray(llmJson, out var generated))
                return;

            var lessons = root
                .SelectTokens("$.weeks[*].modules[*].lessons[*]")
                .OfType<JObject>()
                .ToList();

            if (!lessons.Any() || !generated.Any())
                return;

            foreach (var lesson in lessons)
            {
                lesson[propertyName] = new JArray();
            }

            int lessonIndex = 0;

            foreach (var item in generated.OfType<JObject>())
            {
                var targetLesson = lessons[lessonIndex];

                ((JArray)targetLesson[propertyName]).Add(item);

                lessonIndex++;

                if (lessonIndex >= lessons.Count)
                    lessonIndex = 0;
            }
        }

        public static void MergeWeekArray(
            JObject root,
            string propertyName,
            string llmJson)
        {
            if (!TryParseArray(llmJson, out var generated))
                return;

            var week = root["weeks"]?.First as JObject;
            if (week == null)
                return;

            week[propertyName] = new JArray(generated.OfType<JObject>());
        }

        private static bool TryParseArray(string json, out JArray array)
        {
            array = null!;
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                var token = JToken.Parse(json);
                if (token is not JArray arr)
                    return false;

                array = arr;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
