using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeekController : ControllerBase
    {
        private readonly IWeekService _weekService;

        public WeekController(IWeekService weekService)
        {
            _weekService = weekService;
        }

        [HttpPost("{planId}")]
        public async Task<IActionResult> CreateWeek(Guid planId, [FromBody] WeekDto weekDto)
        {
            try
            {
                var created = await _weekService.CreateWeekAsync(planId, weekDto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to create week: {ex.Message}" });
            }
        }

        [HttpGet("plan/{planId}")]
        public async Task<IActionResult> GetWeeksByPlan(Guid planId)
        {
            try
            {
                var weeks = await _weekService.GetWeeksByPlanAsync(planId);
                return Ok(weeks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to get weeks: {ex.Message}" });
            }
        }

        [HttpGet("{weekId}")]
        public async Task<IActionResult> GetWeek(Guid weekId)
        {
            try
            {
                var week = await _weekService.GetWeekByIdAsync(weekId);
                if (week == null) return NotFound(new { message = "Week not found" });
                return Ok(week);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error fetching week: {ex.Message}" });
            }
        }

        [HttpGet("{weekId}/progress/{userId}")]
        public async Task<IActionResult> GetWeekProgress(Guid weekId, Guid userId)
        {
            try
            {
                var progress = await _weekService.GetWeekProgressAsync(weekId, userId);
                if (progress == null) return NotFound(new { message = "Week or user progress not found" });
                return Ok(progress);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error fetching progress: {ex.Message}" });
            }
        }

        [HttpPut("{weekId}")]
        public async Task<IActionResult> UpdateWeek(Guid weekId, [FromBody] WeekDto weekDto)
        {
            try
            {
                await _weekService.UpdateWeekAsync(weekId, weekDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to update week: {ex.Message}" });
            }
        }

        [HttpDelete("{weekId}")]
        public async Task<IActionResult> DeleteWeek(Guid weekId)
        {
            try
            {
                await _weekService.DeleteWeekAsync(weekId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to delete week: {ex.Message}" });
            }
        }

        [HttpGet("full/{weekId}")]
        public async Task<IActionResult> GetWeekFull(Guid weekId, [FromQuery] Guid? userId = null)
        {
            try
            {
                var week = await _weekService.GetWeekFullByIdAsync(weekId, userId);
                if (week == null) return NotFound(new { message = "Week not found" });
                return Ok(week);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to fetch full week data: {ex.Message}" });
            }
        }

        [HttpGet("full/plan/{planId}")]
        public async Task<IActionResult> GetWeeksFullByPlan(Guid planId, [FromQuery] Guid? userId = null)
        {
            try
            {
                var weeks = await _weekService.GetWeeksFullByPlanAsync(planId, userId);
                return Ok(weeks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to fetch full weeks: {ex.Message}" });
            }
        }
    }
}
