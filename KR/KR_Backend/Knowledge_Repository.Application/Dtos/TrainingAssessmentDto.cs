namespace Knowledge_Repository.Application.Dtos
{
    public class TrainingAssessmentDto
    {
        public string Title { get; set; }
        public string AssessmentType { get; set; }

        public int Difficulty { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        public string LearningObjectives { get; set; }
        public string Description { get; set; }

        public string Metadata { get; set; }
        public List<TrainingAssessmentQuestionDto> Questions { get; set; } = [];
    }

    public class TrainingAssessmentQuestionDto
    {
        public string Question { get; set; }
        public string QuestionType { get; set; }
        public string Options { get; set; }

        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }

        public int? Marks { get; set; }
        public string EvaluationStrategy { get; set; }
        public string Metadata { get; set; }
    }
}
