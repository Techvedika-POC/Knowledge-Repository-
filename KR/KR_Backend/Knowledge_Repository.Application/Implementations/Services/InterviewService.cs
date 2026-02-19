using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

public class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _interviewRepo;
    private readonly ICodingChallengeRepository _codingRepo;
    private readonly IAiService _aiService;

    public InterviewService(
        IInterviewRepository interviewRepo,
        ICodingChallengeRepository codingRepo,
        IAiService aiService)
    {
        _interviewRepo = interviewRepo;
        _codingRepo = codingRepo;
        _aiService = aiService;
    }

    public async Task<Guid> StartInterviewAsync(StartInterviewDto dto)
    {
        var submissions = await _codingRepo
            .GetSubmissionsByUserAsync(dto.UserId);

        var lastSubmission = submissions
            .FirstOrDefault(x => x.ProblemId == dto.ProblemId);

        if (lastSubmission == null)
            throw new Exception("No submission found for this problem.");

        if (lastSubmission.Problem == null)
            throw new Exception("Problem not loaded for submission.");

        var session = new InterviewSession
        {
            InterviewId = Guid.NewGuid(),
            UserId = dto.UserId,
            ProblemId = dto.ProblemId,
            CreatedOn = DateTime.UtcNow,
            Transcript = $"""
Problem:
{lastSubmission.Problem.ProblemStatement}

User's Code:
{lastSubmission.SourceCode}

AI: Let's begin. Explain your approach.
"""
        };

        await _interviewRepo.CreateAsync(session);
        return session.InterviewId;
    }

    public async Task<InterviewResultDto> SendMessageAsync(InterviewMessageDto dto)
    {
        var session = await _interviewRepo.GetByIdAsync(dto.InterviewId);

        session.Transcript += $"User: {dto.Message}\n";
        session.QuestionCount++;

        if (session.QuestionCount < 3)
        {
            var aiInsight = await _aiService.GenerateInsightAsync(
                new GenerateAiInsightRequestDto
                {
                    EntityType = "Interview",
                    InsightType = "MockInterview",
                    EntityId = session.InterviewId,
                    Context = session.Transcript,
                    UserId = session.UserId
                });

            session.Transcript += $"AI: {aiInsight.OutputResult}\n";
            await _interviewRepo.UpdateAsync(session);

            return new InterviewResultDto
            {
                InterviewId = session.InterviewId,
                AiQuestion = aiInsight.OutputResult
            };
        }
        var finalInsight = await _aiService.GenerateInsightAsync(
            new GenerateAiInsightRequestDto
            {
                EntityType = "InterviewEvaluation",
                InsightType = "FinalInterviewScore",
                EntityId = session.InterviewId,
                Context = session.Transcript,
                UserId = session.UserId
            });

        session.CommunicationScore = finalInsight.Score;
        session.FinalFeedback = finalInsight.OutputResult;
        session.IsCompleted = true;
        session.CompletedOn = DateTime.UtcNow;

        await _interviewRepo.UpdateAsync(session);

        return new InterviewResultDto
        {
            InterviewId = session.InterviewId,
            CommunicationScore = session.CommunicationScore,
            AiFeedback = session.FinalFeedback,
            IsCompleted = true
        };
    }

}
