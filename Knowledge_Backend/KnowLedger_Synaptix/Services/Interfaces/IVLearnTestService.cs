using KnowLedger_Synaptix.Dtos;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IVLearnTestService
    {
        Task<VLearnQuestionResponseDto> GenerateQuestionsAsync(VLearnQuestionRequestDto request);
    }
}
