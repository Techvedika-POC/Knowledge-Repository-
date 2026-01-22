
using Newtonsoft.Json.Linq;

namespace TrainingPlanParser.Services.Enrichment.Models
{
    public class EnrichmentPassRequest
    {
        public EnrichmentPassType PassType { get; set; }
        public List<LessonContext> Lessons { get; set; } = new();
        public WeekContext? Week { get; set; }
        public JObject CurrentPlanJson { get; set; } = new();
    }
}
