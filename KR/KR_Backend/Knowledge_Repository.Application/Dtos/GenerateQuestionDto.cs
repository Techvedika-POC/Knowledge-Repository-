namespace Knowledge_Repository.Application.Dtos
{
    public class VLearnQuestionDto
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }  
    }

    public class VLearnQuestionRequestDto
    {
        public string ModuleName { get; set; }
        public string Type { get; set; } = "topicBased"; 
    }

    public class VLearnQuestionResponseDto
    {
        public List<VLearnQuestionDto> Questions { get; set; } = new();
    }
}