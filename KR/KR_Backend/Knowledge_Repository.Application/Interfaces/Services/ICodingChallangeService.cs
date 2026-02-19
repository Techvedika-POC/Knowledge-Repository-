using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Services;

public interface ICodingChallengeService
{
    Task CreateChallengeAsync(CreateCodingChallengeDto dto);
    Task<AiInsightResponseDto> SubmitCodeAsync(SubmitCodeDto dto);
    Task<List<ChallengeProblem>> GetChallengesAsync(Guid challengeId);
    Task<List<ChallengeProblemDto>> GetChallengesByEventAsync(Guid eventId);
    Task<CodingChallengeMetricsDto> GetMetricsAsync();
    Task<List<LeaderboardRowDto>> GetLeaderboardAsync();
}
