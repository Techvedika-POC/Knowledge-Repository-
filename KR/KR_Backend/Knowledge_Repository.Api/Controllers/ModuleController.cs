using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModuleController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpPost("{weekId}")]
        public async Task<IActionResult> CreateModule(Guid weekId, [FromBody] ModuleDto moduleDto)
        {
            var created = await _moduleService.CreateModuleAsync(weekId, moduleDto);
            return Ok(created);
        }

   [HttpGet("week/{weekId}")]
public async Task<IActionResult> GetModulesByWeek(
    Guid weekId,
    [FromQuery] Guid userId)
{
    var modules = await _moduleService
        .GetModulesByWeekAsync(weekId, userId);

    return Ok(modules);
}


        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModuleDetail(Guid moduleId, [FromQuery] Guid userId)
        {
            var module = await _moduleService.GetModuleDetailAsync(moduleId, userId);
            if (module == null) return NotFound();
            return Ok(module);
        }

        [HttpGet("{moduleId}/progress/{userId}")]
        public async Task<IActionResult> GetModuleProgress(Guid moduleId, Guid userId)
        {
            var progress = await _moduleService.GetModuleProgressAsync(moduleId, userId);
            if (progress == null) return NotFound();
            return Ok(progress);
        }

        [HttpPut("{moduleId}")]
        public async Task<IActionResult> UpdateModule(Guid moduleId, [FromBody] ModuleDto moduleDto)
        {
            await _moduleService.UpdateModuleAsync(moduleId, moduleDto);
            return NoContent();
        }

        [HttpDelete("{moduleId}")]
        public async Task<IActionResult> DeleteModule(Guid moduleId)
        {
            await _moduleService.DeleteModuleAsync(moduleId);
            return NoContent();
        }
    }
}
