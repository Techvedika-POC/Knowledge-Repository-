// File: API/Controllers/LearningPlanController.cs
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LearningPlanController : ControllerBase
    {
        private readonly ILearningPlanService _planService;

        public LearningPlanController(ILearningPlanService planService)
        {
            _planService = planService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllPlans()
        {
            return Ok(await _planService.GetAllPlansAsync());
        }

        [HttpGet("{planId}/hierarchy")]
        public async Task<IActionResult> GetPlanHierarchy(Guid planId, [FromQuery] Guid? userId = null)
        {
            var plan = await _planService.GetPlanHierarchyFullAsync(planId, userId);
            return plan == null ? NotFound() : Ok(plan);
        }

        [HttpGet("{planId}/completed/{userId}")]
        public async Task<IActionResult> IsCompleted(Guid planId, Guid userId)
        {
            bool done = await _planService.IsPlanCompletedAsync(planId, userId);
            return Ok(new { planId, userId, completed = done });
        }


        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GeneratePlanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Title required.");

            return Ok(await _planService.GenerateLearningPlanAsync(request.Title, request.UseAI));
        }

        [HttpPost("create-full")]
        public async Task<IActionResult> CreateFull([FromBody] LearningPlanFullDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title required.");

            return Ok(await _planService.CreateFullLearningPlanAsync(dto));
        }


        [HttpPut("{planId}")]
        public async Task<IActionResult> UpdatePlan(Guid planId, [FromBody] LearningPlanFullDto dto)
        {
            dto.PlanId = planId;
            var ok = await _planService.UpdateLearningPlanAsync(dto);
            return ok ? Ok(dto) : NotFound();
        }

        [HttpPut("week/{weekId}")]
        public async Task<IActionResult> UpdateWeek(Guid weekId, [FromBody] WeekFullDto dto)
        {
            dto.WeekId = weekId;
            var ok = await _planService.UpdateWeekAsync(dto);
            return ok ? Ok(dto) : NotFound();
        }

        [HttpPut("module/{moduleId}")]
        public async Task<IActionResult> UpdateModule(Guid moduleId, [FromBody] ModuleDetailDto dto)
        {
            dto.ModuleId = moduleId;
            var ok = await _planService.UpdateModuleAsync(dto);
            return ok ? Ok(dto) : NotFound();
        }

        [HttpPut("lesson/{lessonId}")]
        public async Task<IActionResult> UpdateLesson(Guid lessonId, [FromBody] LessonDto dto)
        {
            dto.LessonId = lessonId;
            var ok = await _planService.UpdateLessonAsync(dto);
            return ok ? Ok(dto) : NotFound();
        }


        [HttpDelete("{planId}")]
        public async Task<IActionResult> DeletePlan(Guid planId)
        {
            var ok = await _planService.DeleteLearningPlanAsync(planId);
            return ok ? Ok() : NotFound();
        }

        [HttpDelete("module/{moduleId}")]
        public async Task<IActionResult> DeleteModule(Guid moduleId)
        {
            var ok = await _planService.DeleteModuleAsync(moduleId);
            return ok ? Ok() : NotFound();
        }
    }

    public class GeneratePlanRequest
    {
        public string Title { get; set; } = string.Empty;
        public bool UseAI { get; set; }
    }
}
