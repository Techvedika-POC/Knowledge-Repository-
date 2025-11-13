using Knowledge_Repository.Application.Dtos;
namespace  Knowledge_Repository.Application.Interfaces.Services
{
    public interface IVLearnTestService
    {
        Task<VLearnQuestionResponseDto> GenerateQuestionsAsync(VLearnQuestionRequestDto request);
    }
}