using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/user-progress")]
    public class UserProgressController : ControllerBase
    {
        private readonly IUserProgressService _service;

        public UserProgressController(
            IUserProgressService service)
        {
            _service = service;
        }

        // -------- PLAN --------

        [HttpPost("{userId}/plan/{planId}/enroll")]
        public async Task<IActionResult> EnrollToPlan(
            Guid userId,
            Guid planId,
            [FromQuery] Guid assignedBy)
        {
            await _service.EnrollUserToPlanAsync(
                userId, planId, assignedBy);
            return NoContent();
        }

        [HttpPost("{userId}/plan/{planId}/start")]
        public async Task<IActionResult> StartPlan(
            Guid userId,
            Guid planId)
        {
            await _service.StartPlanAsync(
                userId, planId);
            return NoContent();
        }

        // -------- LESSON --------

        [HttpPost("{userId}/lesson/{lessonId}/start")]
        public async Task<IActionResult> StartLesson(
            Guid userId,
            Guid lessonId,
            [FromQuery] Guid moduleId)
        {
            await _service.StartLessonAsync(
                userId, lessonId, moduleId);
            return NoContent();
        }

        [HttpPost("{userId}/lesson/{lessonId}/complete")]
        public async Task<IActionResult> CompleteLesson(
            Guid userId,
            Guid lessonId)
        {
            await _service.CompleteLessonAsync(
                userId, lessonId);
            return NoContent();
        }

        // -------- ASSESSMENT --------

        [HttpPost("{userId}/assessment/{assessmentId}/start")]
        public async Task<IActionResult> StartAssessment(
            Guid userId,
            Guid assessmentId,
            [FromQuery] Guid moduleId)
        {
            await _service.StartAssessmentAsync(
                userId, assessmentId, moduleId);
            return NoContent();
        }
        [HttpPost("{userId}/assessment/{assessmentId}/submit")]
        public async Task<ActionResult<AssessmentResultDto>> SubmitAssessment(
            Guid userId,
            Guid assessmentId,
            [FromBody] SubmitAssessmentDto dto)
        {
            dto.UserId = userId;
            dto.AssessmentId = assessmentId;

            var result = await _service.SubmitAssessmentAsync(dto);
            return Ok(result);
        }



        // -------- PROGRESS --------

        [HttpGet("{userId}/module/{moduleId}")]
        public async Task<ActionResult<decimal>> GetModuleProgress(
            Guid userId,
            Guid moduleId)
        {
            return Ok(await _service
                .GetModuleProgressAsync(userId, moduleId));
        }

        [HttpGet("{userId}/plan/{planId}")]
        public async Task<ActionResult<decimal>> GetPlanProgress(
            Guid userId,
            Guid planId)
        {
            return Ok(await _service
                .GetPlanProgressAsync(userId, planId));
        }
    }


}
