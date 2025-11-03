namespace KnowLedger_Synaptix.Dtos
{
    public class VLearnQuestionDto
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }  // optional, not sent to user
    }

    public class VLearnQuestionRequestDto
    {
        public string ModuleName { get; set; }
        public string Type { get; set; } = "topicBased"; // or "contextBased"
    }

    public class VLearnQuestionResponseDto
    {
        public List<VLearnQuestionDto> Questions { get; set; } = new();
    }
}
