
using Knowledge_Repository.Application.Interfaces.Services;
using System.Text.Json;

namespace Knowledge_Repository.Application.Implementations.Services;

public class AiSkillInferenceService : IAiSkillInferenceService
{
    private readonly ILlmClient _llm;

    public AiSkillInferenceService(ILlmClient llm)
    {
        _llm = llm;
    }

    public async Task<Dictionary<string, double>> InferSkillsAsync(string context)
    {
        var prompt = """
You are an AI skill inference engine.

Analyze the following user activity and return ONLY valid JSON:

{
  "Leadership": 40,
  "Communication": 65,
  "Collaboration": 60,
  "Problem Solving": 70,
  "Product Thinking": 55
}

Rules:
- Values 0–100
- No extra text
""";

        var result = await _llm.GenerateAsync(prompt);

        try
        {
            return System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, double>>(result)
                ?? new();
        }
        catch
        {
            return new();
        }
    }
}



