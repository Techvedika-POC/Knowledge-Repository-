using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProgressController : ControllerBase
    {
        private readonly IUserProgressService _service;

        public UserProgressController(IUserProgressService service)
        {
            _service = service;
        }

        [HttpGet("{userId}/plan/{planId}")]
        public async Task<ActionResult<UserProgressDto>> GetUserProgress(Guid userId, Guid planId)
        {
            return Ok(await _service.GetUserProgressAsync(userId, planId));
        }

        [HttpPost("{userId}/week/{weekId}/module/{moduleId}/lesson/{lessonId}/access")]
        public async Task<IActionResult> TrackLesson(Guid userId, Guid weekId, Guid moduleId, Guid lessonId)
        {
            await _service.TrackLessonAccessAsync(userId, moduleId, lessonId);
            return NoContent();
        }

        [HttpPost("{userId}/module/{moduleId}/lesson/{lessonId}/complete")]
        public async Task<IActionResult> CompleteLesson(Guid userId, Guid moduleId, Guid lessonId)
        {
            await _service.MarkLessonCompletedAsync(userId, moduleId, lessonId);
            return NoContent();
        }

        [HttpPost("{userId}/week/{weekId}/module/{moduleId}/test")]
        public async Task<IActionResult> UpdateTestStatus(Guid userId, Guid weekId, Guid moduleId, [FromQuery] string status)
        {
            await _service.UpdateTestStatusAsync(userId, weekId, moduleId, status);
            return NoContent();
        }

        [HttpPost("{userId}/week/{weekId}/module/{moduleId}/complete")]
        public async Task<IActionResult> CompleteModule(Guid userId, Guid weekId, Guid moduleId)
        {
            bool ok = await _service.TryMarkModuleCompletedAsync(userId, weekId, moduleId);
            if (!ok)
                return BadRequest("Cannot complete module: lessons or assessment incomplete.");

            return NoContent();
        }
    }

}
