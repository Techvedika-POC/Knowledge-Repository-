namespace TrainingPlanParser.Services
{
    public interface ILlmService
    {
        Task<string> GenerateStructuredJson(string parsedText);
    }
}
