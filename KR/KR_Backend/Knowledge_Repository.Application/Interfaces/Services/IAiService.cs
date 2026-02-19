using Knowledge_Repository.Application.Dtos;

namespace Knowledge_Repository.Application.Interfaces.Services;

public interface IAiService
{
    Task<AiInsightResponseDto> GenerateInsightAsync(
        GenerateAiInsightRequestDto request
    );

    Task<List<AiChatMessageDto>> ChatAsync(
        AiChatRequestDto request
    );
    Task<Dictionary<string, double>> InferSkillsAsync(
      string context
  );
}
