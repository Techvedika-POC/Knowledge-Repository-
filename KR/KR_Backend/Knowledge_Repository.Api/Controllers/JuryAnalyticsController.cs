using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/jury")]
    public class JuryAnalyticsController : ControllerBase
    {
        private readonly IJuryAnalyticsService _service;

        public JuryAnalyticsController(IJuryAnalyticsService service)
        {
            _service = service;
        }

        [HttpPost("score")]
        public async Task<IActionResult> SubmitScore(
            [FromBody] SubmitJuryScoreDto dto)
        {
            await _service.SubmitScoreAsync(dto);
            return Ok();
        }

        [HttpGet("radar/{teamId}")]
        public async Task<IActionResult> GetRadar(Guid teamId)
        {
            var result = await _service.GetRadarChartAsync(teamId);
            return Ok(result);
        }

        [HttpGet("rankings/{eventId}")]
        public async Task<IActionResult> GetRankings(Guid eventId)
        {
            var result = await _service.GetRankingsAsync(eventId);
            return Ok(result);
        }
    }

}
