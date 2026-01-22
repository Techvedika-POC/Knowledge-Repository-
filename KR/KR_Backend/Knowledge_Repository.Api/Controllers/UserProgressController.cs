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

        // GET USER PROGRESS
        [HttpGet("{userId}/plan/{planId}")]
        public async Task<ActionResult<UserProgressDto>> GetUserProgress(
            Guid userId,
            Guid planId)
        {
            return Ok(await _service.GetUserProgressAsync(userId, planId));
        }

        // TRACK LESSON ACCESS
        [HttpPost("{userId}/module/{moduleId}/lesson/{lessonId}/access")]
        public async Task<IActionResult> TrackLesson(
            Guid userId,
            Guid moduleId,
            Guid lessonId)
        {
            await _service.TrackLessonAccessAsync(userId, moduleId, lessonId);
            return NoContent();
        }

        // MARK LESSON COMPLETED
        [HttpPost("{userId}/module/{moduleId}/lesson/{lessonId}/complete")]
        public async Task<IActionResult> CompleteLesson(
            Guid userId,
            Guid moduleId,
            Guid lessonId)
        {
            await _service.MarkLessonCompletedAsync(userId, moduleId, lessonId);
            return NoContent();
        }

        // UPDATE TEST STATUS
        [HttpPost("{userId}/module/{moduleId}/test")]
        public async Task<IActionResult> UpdateTestStatus(
            Guid userId,
            Guid moduleId,
            [FromQuery] string status)
        {
            await _service.UpdateTestStatusAsync(
                userId,
                Guid.Empty,    
                moduleId,
                status
            );

            return NoContent();
        }

        // COMPLETE MODULE 
        [HttpPost("{userId}/module/{moduleId}/complete")]
        public async Task<IActionResult> CompleteModule(
            Guid userId,
            Guid moduleId)
        {
            bool ok = await _service.TryMarkModuleCompletedAsync(
                userId,
                Guid.Empty,     
                moduleId
            );

            if (!ok)
                return BadRequest("Cannot complete module: lessons or assessment incomplete.");

            return NoContent();
        }
    }
}
