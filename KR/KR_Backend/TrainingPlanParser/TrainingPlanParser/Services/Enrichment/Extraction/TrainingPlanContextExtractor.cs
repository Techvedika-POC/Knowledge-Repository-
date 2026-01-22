using Newtonsoft.Json.Linq;
using TrainingPlanParser.Services.Enrichment.Models;

namespace TrainingPlanParser.Services.Enrichment.Extraction
{
    public static class TrainingPlanContextExtractor
    {
        // =====================================================
        // LESSON CONTEXT (Topic-level enrichment)
        // =====================================================
        public static List<LessonContext> ExtractLessons(JObject root)
        {
            var lessons = new List<LessonContext>();

            var lessonTokens = root
                .SelectTokens("$.weeks[*].modules[*].lessons[*]")
                .OfType<JObject>();

            foreach (var lesson in lessonTokens)
            {
                var lessonId =
                    lesson["lessonId"]?.ToObject<Guid>()
                    ?? Guid.NewGuid();

                var title =
                    lesson["title"]?.ToString()
                    ?? "Untitled Lesson";

                var summary =
                    lesson["overview"]?.ToString()
                    ?? lesson["content"]?.ToString()
                    ?? "";

                summary = Trim(summary, 300);

                lessons.Add(new LessonContext
                {
                    LessonId = lessonId,
                    Title = title,
                    Summary = summary
                });
            }

            return lessons;
        }

        // =====================================================
        // WEEK CONTEXT (Week-level assignment)
        // =====================================================
        public static WeekContext ExtractWeek(JObject root)
        {
            var weekToken = root
                .SelectToken("$.weeks[0]") as JObject;

            if (weekToken == null)
                throw new InvalidOperationException("No week found in training plan JSON.");

            var weekId =
                weekToken["weekId"]?.ToObject<Guid>()
                ?? Guid.NewGuid();

            var title =
                weekToken["description"]?.ToString()
                ?? weekToken["learningObjectives"]?.ToString()
                ?? "Weekly Assignment";

            title = Trim(title, 200);

            return new WeekContext
            {
                WeekId = weekId,
                Title = title,
                Lessons = ExtractLessons(root)
            };
        }
        private static string Trim(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            value = value.Replace("\n", " ").Replace("\r", " ");
            return value.Length <= maxLength
                ? value
                : value[..maxLength];
        }
    }
}
