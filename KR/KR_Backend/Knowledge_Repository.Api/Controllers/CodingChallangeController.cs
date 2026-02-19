using Microsoft.AspNetCore.Mvc;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;

namespace Knowledge_Repository.API.Controllers;

[ApiController]
[Route("api/coding")]
public class CodingChallengeController : ControllerBase
{
    private readonly ICodingChallengeService _service;

    public CodingChallengeController(ICodingChallengeService service)
    {
        _service = service;
    }

    [HttpGet("challenges/{challengeId}")]
    public async Task<IActionResult> GetChallenges(Guid challengeId)
    {
        var result = await _service.GetChallengesAsync(challengeId);
        return Ok(result);
    }

    [HttpPost("challenge")]
    public async Task<IActionResult> CreateChallenge(
        [FromBody] CreateCodingChallengeDto dto)
    {
        await _service.CreateChallengeAsync(dto);
        return Ok();
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitCode(
      [FromBody] SubmitCodeDto dto)
    {
        var result = await _service.SubmitCodeAsync(dto);

        return Ok(new
        {
            submissionId = result.EntityId,
            score = result.Score,
            feedback = result.OutputResult
        });
    }

    [HttpGet("challenges/by-event/{eventId}")]
    public async Task<IActionResult> GetChallengesByEvent(Guid eventId)
    {
        var result = await _service.GetChallengesByEventAsync(eventId);
        return Ok(result);
    }
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _service.GetMetricsAsync();
        return Ok(metrics);
    }
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        return Ok(await _service.GetLeaderboardAsync());
    }



}

