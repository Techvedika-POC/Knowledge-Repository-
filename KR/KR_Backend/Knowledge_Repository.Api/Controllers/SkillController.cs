using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/skills")]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _service;

        public SkillController(ISkillService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] AddSkillDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Skill name is required.");

            await _service.AddSkillAsync(dto);
            return Ok();
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateUserSkill(
            [FromBody] UpdateUserSkillDto dto)
        {
            if (dto.UserId == Guid.Empty)
                return BadRequest("UserId is required.");

            if (string.IsNullOrWhiteSpace(dto.SkillName))
                return BadRequest("SkillName is required.");

            if (dto.Proficiency < 0 || dto.Proficiency > 100)
                return BadRequest("Proficiency must be between 0 and 100.");

            await _service.UpdateUserSkillAsync(dto);
            return Ok();
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserSkills([FromRoute] Guid userId)
        {
            var result = await _service.GetUserSkillsAsync(userId);
            return Ok(result);
        }

        [HttpGet("summary/{userId:guid}")]
        public async Task<IActionResult> GetSummary([FromRoute] Guid userId)
        {
            var result = await _service.GetSkillSummaryAsync(userId);
            return Ok(result);
        }
    }
}
