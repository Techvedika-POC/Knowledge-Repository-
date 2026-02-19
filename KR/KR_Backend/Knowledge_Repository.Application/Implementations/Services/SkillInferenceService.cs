using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;

namespace Knowledge_Repository.Application.Implementations.Services;

public class SkillInferenceService : ISkillInferenceService
{
    private readonly IAiSkillInferenceService _aiSkillInference;
    private readonly ISkillService _skillService;

    public SkillInferenceService(
        IAiSkillInferenceService aiSkillInference,
        ISkillService skillService)
    {
        _aiSkillInference = aiSkillInference;
        _skillService = skillService;
    }

    public async Task InferFromActivityAsync(Guid userId, string activityContext)
    {
        var inferred = await _aiSkillInference.InferSkillsAsync(activityContext);

        foreach (var (skill, value) in inferred)
        {
            await _skillService.UpdateUserSkillAsync(new UpdateUserSkillDto
            {
                UserId = userId,
                SkillName = skill,
                Proficiency = value
            });
        }
    }
}

