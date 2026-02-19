using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/manager")]
    [Authorize(Roles = "Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _service;

        public ManagerController(IManagerService service)
        {
            _service = service;
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = await _service.GetAllLearningPlansAsync();
            return Ok(plans);
        }

        [HttpPost("assign-plan")]
        public async Task<IActionResult> AssignPlan([FromBody] AssignLearningPlanDto dto)
        {
            var managerIdClaim = User.FindFirst("sub")
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("id");

            if (managerIdClaim == null)
                return Unauthorized("Manager ID not found in token");

            var managerId = Guid.Parse(managerIdClaim.Value);

            await _service.AssignLearningPlanAsync(
                dto.PlanId,
                managerId,
                dto.UserIds
            );

            return Ok(new { message = "Plan assigned successfully" });
        }

        [HttpGet("plan/{planId}/progress")]
        public async Task<IActionResult> GetPlanProgress(Guid planId)
        {
            var result = await _service.GetPlanProgressAsync(planId);
            return Ok(result);
        }
    }
}
