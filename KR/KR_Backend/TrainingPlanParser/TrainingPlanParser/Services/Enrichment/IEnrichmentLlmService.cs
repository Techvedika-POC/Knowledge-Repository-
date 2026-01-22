namespace TrainingPlanParser.Services.Enrichment
{
    public interface IEnrichmentLlmService
    {
        Task<string> GenerateAsync(string prompt);
        Task<string> GenerateBatchAsync(string prompt);
    }

}
