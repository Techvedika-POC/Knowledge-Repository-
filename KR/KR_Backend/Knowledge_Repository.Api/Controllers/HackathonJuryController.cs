using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/hackathon/jury")]
    public class HackathonJuryController : ControllerBase
    {
        private readonly IHackathonJuryService _service;

        public HackathonJuryController(IHackathonJuryService service)
        {
            _service = service;
        }

        [HttpGet("teams/{eventId}")]
        public async Task<IActionResult> GetTeams(Guid eventId)
        {
            return Ok(await _service.GetTeamsForJuryAsync(eventId));
        }

        [HttpPost("ai-evaluate/{teamId}")]
        public async Task<IActionResult> Evaluate(Guid teamId, [FromQuery] Guid eventId)
        {
            return Ok(await _service.EvaluateIdeaAsync(teamId, eventId));
        }
    }
}
