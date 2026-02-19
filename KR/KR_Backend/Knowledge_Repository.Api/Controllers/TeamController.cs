using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/team")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet("my/{eventId}/{userId}")]
        public async Task<IActionResult> GetMyTeam(Guid eventId, Guid userId)
        {
            var team = await _teamService.GetMyTeamForEvent(eventId, userId);

            if (team == null)
                return NotFound();

            return Ok(team);
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetTeamsForEvent(Guid eventId)
        {
            return Ok(await _teamService.GetTeamsForEvent(eventId));
        }
        [HttpPost("{teamId}/add-member")]
        public async Task<IActionResult> AddMember(
            Guid teamId,
            [FromQuery] Guid creatorId,
            [FromQuery] string email)
        {
            await _teamService.AddMemberAsync(teamId, creatorId, email);
            return Ok();
        }

        [HttpDelete("{teamId}/remove-member/{memberId}")]
        public async Task<IActionResult> RemoveMember(
            Guid teamId,
            Guid memberId,
            [FromQuery] Guid creatorId)
        {
            await _teamService.RemoveMemberAsync(teamId, creatorId, memberId);
            return Ok();
        }
    }
}
