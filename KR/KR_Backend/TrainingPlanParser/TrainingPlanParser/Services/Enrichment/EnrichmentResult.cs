using TrainingPlanParser.Services.Enrichment;
using TrainingPlanParser.Services.Pipeline.Models;

public class EnrichmentResult
{
    public string EnrichedJson { get; set; } = string.Empty;
    public TrainingPlanPipelineResult Evaluation { get; set; } = new();

    public List<EnrichedFieldResult> EnrichedFields { get; set; } = new();
}
