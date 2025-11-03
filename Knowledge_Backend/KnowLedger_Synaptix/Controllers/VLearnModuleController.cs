using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Implementations;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KnowLedger_Synaptix.Controllers
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

        [HttpGet("{topicId}/{userId}")]
        public async Task<IActionResult> GetModulesByTopicAndUser(Guid topicId, Guid userId)
        {
            var modules = await _vlearnModuleService.GetModulesByTopicAndUserAsync(topicId, userId);
            return Ok(modules);
        }

        [HttpPost("update-test-status")]
        public async Task<IActionResult> UpdateTestStatus([FromBody] VLearnTestResultDto result)
        {
            if (result == null) return BadRequest("Invalid data");

            var success = await _vlearnModuleService.UpdateTestStatusAsync(result);
            return success
                ? Ok(new { message = "Status updated successfully" })
                : StatusCode(500, "Failed to update");
        }
    }
}
