using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/hackathon/idea")]
    public class HackathonIdeaController : ControllerBase
    {
        private readonly IIdeaService _ideaService;

        public HackathonIdeaController(IIdeaService ideaService)
        {
            _ideaService = ideaService;
        }

        // SUBMIT IDEA
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitIdeaDto dto)
        {
            await _ideaService.SubmitIdeaAsync(dto);
            return Ok("Idea submitted successfully");
        }

        // GET IDEA BY TEAM
        [HttpGet("by-team/{teamId}")]
        public async Task<IActionResult> GetByTeam(Guid teamId)
        {
            var idea = await _ideaService.GetByTeamAsync(teamId);
            return Ok(idea);
        }
    }

}
