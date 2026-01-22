namespace TrainingPlanParser.Services.Evaluation.RuleBased
{
    public class ExpectedStructureData
    {
        public List<string> Weeks { get; set; } = new();
        public Dictionary<string, List<string>> ModulesPerWeek { get; set; } = new();
    }
}
