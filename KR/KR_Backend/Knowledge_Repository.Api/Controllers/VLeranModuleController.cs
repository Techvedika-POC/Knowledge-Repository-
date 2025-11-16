using KnowLedger_Synaptix.Dtos;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VLearnModuleController : ControllerBase
    {
        private readonly IVLearnModuleService _vlearnModuleService;

        public VLearnModuleController(IVLearnModuleService vlearnModuleService)
        {
            _vlearnModuleService = vlearnModuleService;
        }

        // Public: get modules (no user progress)
        [HttpGet("topic/{topicId:guid}/modules")]
        [AllowAnonymous]
        public async Task<IActionResult> GetModulesByTopic(Guid topicId)
        {
            var modules = await _vlearnModuleService.GetModulesByTopicAsync(topicId);
            return Ok(modules);
        }
        // GET api/vlearnmodule/topic/{topicId}/modules/me
        [HttpGet("topic/{topicId:guid}/modules/me")]
        [Authorize]
        public async Task<IActionResult> GetModulesByTopicForCurrentUser(Guid topicId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var modules = await _vlearnModuleService.GetModulesByTopicAndUserAsync(topicId, userId);
            return Ok(modules);
        }
        // POST api/vlearnmodule/topic/{topicId}/modules
        [HttpPost("topic/{topicId:guid}/modules")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateModule(Guid topicId, [FromBody] CreateModuleDto dto)
        {
            if (dto == null) return BadRequest("Request body is required.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var createdBy = GetCurrentUserId();
                var created = await _vlearnModuleService.AddModuleAsync(topicId, dto, createdBy);
                return CreatedAtAction(nameof(GetModulesByTopic), new { topicId = created.TopicId }, created);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (InvalidOperationException iex)
            {
                return Conflict(iex.Message);
            }
            catch (Exception ex)
            {
                // Optional: log
                return StatusCode(500, ex.Message);
            }
        }
        // POST api/vlearnmodule/update-test-status
        [HttpPost("update-test-status")]
        [Authorize]
        public async Task<IActionResult> UpdateTestStatus([FromBody] VLearnTestResultDto result)
        {
            if (result == null) return BadRequest("Invalid data");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            // enforce that non-admins only update their own status
            if (!User.IsInRole("Admin"))
            {
                result.UserId = userId;
            }

            try
            {
                var success = await _vlearnModuleService.UpdateTestStatusAsync(result);
                return success ? Ok(new { message = "Status updated successfully" }) : StatusCode(500, "Failed to update");
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private Guid GetCurrentUserId()
        {
            var idClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "user_id");
            if (idClaim == null) return Guid.Empty;
            return Guid.TryParse(idClaim.Value, out var g) ? g : Guid.Empty;
        }
    }
}
