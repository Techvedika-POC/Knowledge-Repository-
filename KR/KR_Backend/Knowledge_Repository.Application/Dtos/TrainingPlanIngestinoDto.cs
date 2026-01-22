namespace Knowledge_Repository.Application.Dtos
{
    public class TrainingPlanIngestionDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Overview { get; set; }
        public string Objectives { get; set; }
        public int DurationWeeks { get; set; }
        public int TotalDays { get; set; }

        public List<string> Prerequisites { get; set; }
        public List<string> TechnicalRequirements { get; set; }
        public List<string> SelfAssessmentChecklist { get; set; }

        public List<TrainingWeekDto> Weeks { get; set; }
    }
}
