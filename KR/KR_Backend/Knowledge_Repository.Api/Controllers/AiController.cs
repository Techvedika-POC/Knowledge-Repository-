using Microsoft.AspNetCore.Mvc;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;

namespace Knowledge_Repository.API.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("insight")]
    public async Task<IActionResult> GenerateInsight(
        [FromBody] GenerateAiInsightRequestDto request)
    {
        var result = await _aiService.GenerateInsightAsync(request);
        return Ok(result);
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] AiChatRequestDto request)
    {
        var conversation = await _aiService.ChatAsync(request);
        return Ok(conversation);
    }
}
