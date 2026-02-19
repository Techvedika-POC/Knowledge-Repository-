using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Knowledge_Repository.Infrastructure.Repositories;

public class CodingChallengeRepository : ICodingChallengeRepository
{
    private readonly Knowledge_Repository_dbContext _context;

    public CodingChallengeRepository(Knowledge_Repository_dbContext context)
    {
        _context = context;
    }

    public async Task CreateChallengeAsync(CodingChallenge challenge)
    {
        _context.CodingChallenges.Add(challenge);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ChallengeProblem>> GetProblemsByChallengeAsync(Guid challengeId)
    {
        return await _context.ChallengeProblems
            .Where(x => x.ChallengeId == challengeId)
            .ToListAsync();
    }

    public async Task<CodingChallenge?> GetChallengeByEventAsync(Guid eventId)
    {
        return await _context.CodingChallenges
            .Include(c => c.ChallengeProblems)
            .FirstOrDefaultAsync(c => c.EventId == eventId);
    }


    public async Task AddSubmissionAsync(CodeSubmission submission)
    {
        _context.CodeSubmissions.Add(submission);
        await _context.SaveChangesAsync();
    }
    public async Task<CodeSubmission?> GetLastSubmissionAsync(
    Guid userId, Guid problemId)
    {
        return await _context.CodeSubmissions
            .Include(x => x.Problem)   
            .Where(x =>
                x.UserId == userId &&
                x.ProblemId == problemId)
            .OrderByDescending(x => x.SubmittedOn)
            .FirstOrDefaultAsync();
    }
    public async Task<CodingChallengeMetricsDto> GetMetricsAsync()
    {
        var activeChallenges = await _context.CodingChallenges.CountAsync();

        var participants = await _context.CodeSubmissions
            .Select(x => x.UserId)
            .Union(_context.InterviewSessions.Select(x => x.UserId))
            .Distinct()
            .CountAsync();

        var topCodeScore = await _context.CodeSubmissions
            .Where(x => x.Score != null)
            .MaxAsync(x => (double?)x.Score) ?? 0;

        var topInterviewScore = await _context.InterviewSessions
            .Where(x => x.CommunicationScore != null)
            .MaxAsync(x => (double?)x.CommunicationScore) ?? 0;

        var topScore = Math.Max(topCodeScore, topInterviewScore);

        var difficulty = await _context.CodingChallenges
            .Select(x => x.Difficulty)
            .FirstOrDefaultAsync() ?? "Mixed";

        return new CodingChallengeMetricsDto
        {
            ActiveChallenges = activeChallenges,
            Participants = participants,
            TopScore = topScore,
            Difficulty = difficulty
        };
    }
    public async Task<List<LeaderboardRawRow>> GetLeaderboardRawAsync()
    {
        var data = await
            (from u in _context.Users
             select new LeaderboardRawRow
             {
                 UserId = u.UserId,
                 Name = u.Name,

                 BestCode = _context.CodeSubmissions
                     .Where(s => s.UserId == u.UserId)
                     .Max(x => (double?)x.Score) ?? 0,

                 Submissions = _context.CodeSubmissions
                     .Count(s => s.UserId == u.UserId),

                 LastCode = _context.CodeSubmissions
                     .Where(s => s.UserId == u.UserId)
                     .Max(x => (DateTime?)x.SubmittedOn),

                 BestInterview = _context.InterviewSessions
                     .Where(i => i.UserId == u.UserId && i.IsCompleted)
                     .Max(x => (double?)x.CommunicationScore) ?? 0,

                 LastInterview = _context.InterviewSessions
                     .Where(i => i.UserId == u.UserId && i.IsCompleted)
                     .Max(x => (DateTime?)x.CompletedOn)
             })
            .ToListAsync();

        return data;
    }

    public async Task UpdateSubmissionAsync(CodeSubmission submission)
    {
        _context.CodeSubmissions.Update(submission);
        await _context.SaveChangesAsync();
    }


    public async Task<List<CodeSubmission>> GetSubmissionsByUserAsync(Guid userId)
    {
        return await _context.CodeSubmissions
            .Include(x => x.Problem)  
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.SubmittedOn)
            .ToListAsync();
    }

}
