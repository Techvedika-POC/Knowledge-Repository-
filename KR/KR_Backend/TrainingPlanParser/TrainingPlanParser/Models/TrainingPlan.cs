namespace TrainingPlanParser.Models
{
    public class TrainingPlan
    {
        public string Title { get; set; }
        public string Duration { get; set; }
        public List<string> Prerequisites { get; set; }
        public List<TrainingModule> Modules { get; set; }
        public List<string> Assignments { get; set; }
    }

    public class TrainingModule
    {
        public string Name { get; set; }
        public List<string> LearningObjectives { get; set; }
        public List<TrainingWeek> Weeks { get; set; }
    }

    public class TrainingWeek
    {
        public string WeekName { get; set; }
        public List<string> Topics { get; set; }
        public List<string> Days { get; set; }
    }
}
