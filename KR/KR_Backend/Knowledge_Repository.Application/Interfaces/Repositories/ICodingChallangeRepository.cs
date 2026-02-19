using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Repositories;

public interface ICodingChallengeRepository
{
    Task CreateChallengeAsync(CodingChallenge challenge);
    Task<List<ChallengeProblem>> GetProblemsByChallengeAsync(Guid challengeId);
    Task<CodingChallenge?> GetChallengeByEventAsync(Guid eventId);
    Task<CodingChallengeMetricsDto> GetMetricsAsync();
    Task UpdateSubmissionAsync(CodeSubmission submission);
    Task<List<LeaderboardRawRow>> GetLeaderboardRawAsync();

    Task<CodeSubmission?> GetLastSubmissionAsync(
       Guid userId, Guid problemId);
    Task AddSubmissionAsync(CodeSubmission submission);
    Task<List<CodeSubmission>> GetSubmissionsByUserAsync(Guid userId);
}
