using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Dtos.Mentor;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MentorController : ControllerBase
    {
        private readonly IMentorService _mentorService;

        public MentorController(IMentorService mentorService)
        {
            _mentorService = mentorService;
        }

        [HttpGet("{mentorId}/teams")]
        public async Task<IActionResult> GetTeamsForMentor(Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                return BadRequest("Invalid mentor ID.");

            var teams = await _mentorService.GetTeamsForMentorAsync(mentorId);
            return Ok(teams);
        }


        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetTeamDetails(Guid teamId)
        {
            if (teamId == Guid.Empty)
                return BadRequest("Invalid team ID.");

            var teamDetails = await _mentorService.GetTeamDetailsAsync(teamId);
            if (teamDetails == null)
                return NotFound("Team not found.");

            return Ok(teamDetails);
        }
    }
}
