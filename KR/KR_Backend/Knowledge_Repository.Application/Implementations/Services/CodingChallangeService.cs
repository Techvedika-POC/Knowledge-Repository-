using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Implementations.Services;

public class CodingChallengeService : ICodingChallengeService
{
    private readonly ICodingChallengeRepository _repo;
    private readonly IAiService _aiService;

    public CodingChallengeService(
        ICodingChallengeRepository repo,
        IAiService aiService)
    {
        _repo = repo;
        _aiService = aiService;
    }

    public async Task CreateChallengeAsync(CreateCodingChallengeDto dto)
    {
        var challenge = new CodingChallenge
        {
            ChallengeId = Guid.NewGuid(),
            EventId = dto.EventId,
            Title = dto.Title,
            Difficulty = dto.Difficulty,
            TimeLimitMinutes = dto.TimeLimitMinutes
        };

        await _repo.CreateChallengeAsync(challenge);
    }

    public async Task<AiInsightResponseDto> SubmitCodeAsync(SubmitCodeDto dto)
    {
        var submission = new CodeSubmission
        {
            SubmissionId = Guid.NewGuid(),
            ProblemId = dto.ProblemId,
            UserId = dto.UserId,
            Language = dto.Language,
            SourceCode = dto.SourceCode,
            SubmittedOn = DateTime.UtcNow
        };

        await _repo.AddSubmissionAsync(submission);

        var insight = await _aiService.GenerateInsightAsync(
            new GenerateAiInsightRequestDto
            {
                EntityType = "Code",
                InsightType = "CodeReview",
                EntityId = submission.SubmissionId,
                Context = dto.SourceCode,
                UserId = dto.UserId
            });

        submission.Score = insight.Score;
        await _repo.UpdateSubmissionAsync(submission);

        return insight;
    }


    public async Task<List<ChallengeProblemDto>> GetChallengesByEventAsync(Guid eventId)
    {
        // 1. Get challenge for event
        var challenge = await _repo.GetChallengeByEventAsync(eventId);

        if (challenge == null)
            return new List<ChallengeProblemDto>();

        // 2. Get problems for that challenge
        var problems = await _repo.GetProblemsByChallengeAsync(challenge.ChallengeId);

        return problems.Select(p => new ChallengeProblemDto
        {
            ProblemId = p.ProblemId,
            Title = p.Title,
            ProblemStatement = p.ProblemStatement
        }).ToList();
    }

    public async Task<List<ChallengeProblem>> GetChallengesAsync(Guid challengeId)
    {
        return await _repo.GetProblemsByChallengeAsync(challengeId);
    }
    public async Task<CodingChallengeMetricsDto> GetMetricsAsync()
    {
        return await _repo.GetMetricsAsync();
    }
    public async Task<List<LeaderboardRowDto>> GetLeaderboardAsync()
    {
        var raw = await _repo.GetLeaderboardRawAsync();

        var ranked = raw
            .Select(x =>
            {
                var overall = Math.Max(x.BestCode, x.BestInterview);

                var lastActive =
                    x.LastCode ??
                    x.LastInterview ??
                    DateTime.MinValue;

                return new LeaderboardRowDto
                {
                    UserId = x.UserId,
                    UserName = x.Name,
                    BestCodeScore = x.BestCode,
                    BestInterviewScore = x.BestInterview,
                    OverallScore = overall,
                    Submissions = x.Submissions,
                    LastActive = lastActive
                };
            })
            .OrderByDescending(x => x.OverallScore)
            .Take(50)
            .ToList();

        for (int i = 0; i < ranked.Count; i++)
            ranked[i].Rank = i + 1;

        return ranked;
    }



}

