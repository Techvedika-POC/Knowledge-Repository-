namespace TrainingPlanParser.Services.Pipeline.Models
{
    public class EvaluationReportDto
    {
        public string Phase { get; set; } = "Evaluation";

        public object Summary { get; set; }

        public object RuleBased { get; set; }
        public object ML { get; set; }
        public object Hybrid { get; set; }

        public string LlmStructuredOutput { get; set; }

        public object Payload { get; set; }
    }
}
