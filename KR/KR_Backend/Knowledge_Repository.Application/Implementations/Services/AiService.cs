
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Knowledge_Repository.Application.Implementations.Services;

public class AiService : IAiService
{
    private readonly IAiRepository _repo;
    private readonly ILlmClient _llm;
    private readonly ILearningEventService _learningEventService;

    public AiService(
        IAiRepository repo,
        ILlmClient llm,
        ILearningEventService learningEventService)
    {
        _repo = repo;
        _llm = llm;
        _learningEventService = learningEventService;
    }
    public async Task<AiInsightResponseDto> GenerateInsightAsync(
        GenerateAiInsightRequestDto request)
    {
        var prompt = BuildPrompt(request);
        var result = await _llm.GenerateAsync(prompt);

        double score = 0;
        var match = Regex.Match(result, @"SCORE:\s*(\d+)");
        if (match.Success)
            score = double.Parse(match.Groups[1].Value);

        var insight = new AiInsight
        {
            AiInsightId = Guid.NewGuid(),
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            InputContext = request.Context,
            OutputResult = result,
            Score = score,
            UserId = request.UserId,
            EventId = request.EventId,
            TeamId = request.TeamId,
            InsightType = request.InsightType ?? "General",
            CreatedOn = DateTime.UtcNow
        };

        await _repo.SaveInsightAsync(insight);

        return new AiInsightResponseDto
        {
            AiInsightId = insight.AiInsightId,
            EntityType = insight.EntityType,
            EntityId = insight.EntityId,
            InsightType = insight.InsightType,
            OutputResult = insight.OutputResult,
            Score = (float?)insight.Score,
            CreatedOn = insight.CreatedOn
        };
    }
    public async Task<List<AiChatMessageDto>> ChatAsync(
        AiChatRequestDto request)
    {
        await _repo.SaveConversationAsync(new AiConversation
        {
            ConversationId = Guid.NewGuid(),
            UserId = request.UserId,
            EventId = request.EventId,
            TeamId = request.TeamId,
            Role = "user",
            Message = request.Message,
            CreatedOn = DateTime.UtcNow
        });
        var history = await _repo.GetConversationAsync(
            request.UserId, request.EventId);

        var context = string.Join(
            "\n",
            history.Select(x => $"{x.Role}: {x.Message}")
        );

        var prompt =
$@"You are an AI mentor and assistant.

Conversation so far:
{context}

Respond helpfully to the user.";

        var aiResponse = await _llm.GenerateAsync(prompt);

        // Save AI response
        await _repo.SaveConversationAsync(new AiConversation
        {
            ConversationId = Guid.NewGuid(),
            UserId = request.UserId,
            EventId = request.EventId,
            TeamId = request.TeamId,
            Role = "assistant",
            Message = aiResponse,
            CreatedOn = DateTime.UtcNow
        });
 

        // Return updated conversation
        var updated = await _repo.GetConversationAsync(
            request.UserId, request.EventId);
        var chatMetadata = new
        {
            userMessage = request.Message,
            aiResponse = aiResponse
        };

        await _learningEventService.LogAndProcessAsync(
            userId: request.UserId,
            eventType: "MENTOR_CHAT",
            entityType: "Chat",
            entityId: null, 
            metadata: JsonSerializer.Serialize(chatMetadata) 
        );

        return updated.Select(x => new AiChatMessageDto
        {
            Role = x.Role,
            Message = x.Message,
            CreatedOn = x.CreatedOn
        }).ToList();
    }

    public async Task<Dictionary<string, double>> InferSkillsAsync(
        string context)
    {
        var prompt =
$@"You are an AI skill inference engine.

Based on the following user activity, infer demonstrated skills.

Activity:
{context}

Return ONLY valid JSON in this format:

{{
  ""Leadership"": 40,
  ""Communication"": 65,
  ""Collaboration"": 60,
  ""Problem Solving"": 70,
  ""Product Thinking"": 55
}}

Rules:
- Values must be between 0 and 100
- Include only skills you are confident about
- No explanations, no extra text";

        var result = await _llm.GenerateAsync(prompt);

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<
                Dictionary<string, double>>(result)
                ?? new Dictionary<string, double>();
        }
        catch
        {
            return new Dictionary<string, double>();
        }
    }

    private string BuildPrompt(GenerateAiInsightRequestDto request)
    {
        return request.EntityType switch
        {
            "Idea" =>
$@"You are a startup mentor.

Evaluate this hackathon idea:
{request.Context}

Provide:
1. Strengths
2. Weaknesses
3. Market potential
4. Technical feasibility
5. Improvement suggestions",

            "Code" =>
$@"You are a senior software engineer.

Review the following code:
{request.Context}

Return your response in EXACTLY this format:

SCORE: <number between 0 and 100>

FEEDBACK:
- Correctness:
- Time complexity:
- Space complexity:
- Bugs or issues:
- Optimization suggestions:",

            "InterviewEvaluation" =>
$@"You are a senior technical interviewer.

Based on this full interview transcript:

{request.Context}

Return your response in EXACTLY this format:

SCORE: <number between 0 and 100>

FEEDBACK:
- Technical understanding:
- Problem solving:
- Communication:
- Strengths:
- Weaknesses:
- Final verdict:",

            _ =>
$@"You are an expert AI assistant.

Analyze the following:
{request.Context}

Provide actionable insights."
        };
    }
}
