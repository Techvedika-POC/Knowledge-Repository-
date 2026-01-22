namespace TrainingPlanParser.Services.Evaluation.RuleBased
{
    public class MissingFieldRequest
    {
        public string Level { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;

        public bool IsCritical { get; set; }
        public string JsonPath { get; set; } = string.Empty;
        public string ContextSummary { get; set; } = string.Empty;
    }
}
