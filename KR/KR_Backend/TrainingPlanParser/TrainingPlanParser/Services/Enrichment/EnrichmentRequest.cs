using TrainingPlanParser.Services.Evaluation.RuleBased;

public class EnrichmentRequest
{
    public string RawText { get; set; }
    public string StructuredText { get; set; }

    public string LlmJson { get; set; }

    public List<MissingFieldRequest> MissingFields { get; set; }
}
