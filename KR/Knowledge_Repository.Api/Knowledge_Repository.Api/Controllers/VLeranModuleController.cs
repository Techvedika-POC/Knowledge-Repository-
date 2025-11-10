using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using KnowLedger_Synaptix.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
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

        /// <summary>
        /// Retrieves modules for a specific topic and user.
        /// </summary>
        [HttpGet("{topicId}/{userId}")]
        public async Task<IActionResult> GetModulesByTopicAndUser(Guid topicId, Guid userId)
        {
            var modules = await _vlearnModuleService.GetModulesByTopicAndUserAsync(topicId, userId);
            return Ok(modules);
        }

        /// <summary>
        /// Updates the test status for a module.
        /// </summary>
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
