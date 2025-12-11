using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos
{
    public class AssessmentDto
    {
        public Guid AssessmentId { get; set; }
        public Guid? ModuleId { get; set; }
        public Guid TopicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Difficulty { get; set; } = 0;
        public bool IsAiGenerated { get; set; } = false;
        public string Metadata { get; set; } = "{}";
        public string Description { get; set; } = string.Empty;
        public string LearningObjectives { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public int EstimatedDurationMinutes { get; set; } = 0;
        public List<AssessmentQuestionDto> Questions { get; set; } = new();
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class AssessmentQuestionDto
    {
        public Guid QuestionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Options { get; set; } = "[]";  
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Hint { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string Metadata { get; set; } = "{}";
        public bool IsAiGenerated { get; set; } = false;
    }
    public class SubmitAssessmentDto
    {
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public Guid WeekId { get; set; }

        // JSON dictionary: { "questionId": "userAnswer" }
        public string UserAnswers { get; set; } = "{}";
    }

    public class AssessmentResultDto
        {
            public Guid ResultId { get; set; }
            public Guid UserId { get; set; }
            public Guid AssessmentId { get; set; }

            public string UserAnswers { get; set; } = "[]";
            public double ScorePercentage { get; set; }
            public bool Passed { get; set; }
            public bool IsCompleted { get; set; }

            public DateTime AttemptedOn { get; set; }
            public DateTime? UpdatedOn { get; set; }
        }

}
