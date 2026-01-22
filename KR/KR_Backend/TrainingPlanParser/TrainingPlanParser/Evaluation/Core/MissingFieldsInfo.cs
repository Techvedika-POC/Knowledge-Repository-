namespace TrainingPlanParser.Services.Evaluation.Core
{
    public class MissingFieldInfo
    {
        public string Level { get; set; }
        public string Scope { get; set; }
        public string Field { get; set; }
        public bool IsCritical { get; set; }
    }
}
