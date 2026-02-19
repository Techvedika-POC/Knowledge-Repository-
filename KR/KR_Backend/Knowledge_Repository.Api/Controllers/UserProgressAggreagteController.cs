using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProgressAggregateController : ControllerBase
    {
        private readonly IUserProgressService _progressService;
        private readonly IModuleService _moduleService;
        private readonly IModuleRepository _moduleRepo;

        public UserProgressAggregateController(
            IUserProgressService progressService,
            IModuleService moduleService,
            IModuleRepository moduleRepo)
        {
            _progressService = progressService;
            _moduleService = moduleService;
            _moduleRepo = moduleRepo;
        }

        [HttpGet("{userId}/plan/{planId}")]
        public async Task<IActionResult> GetPlanAggregate(
       Guid userId,
       Guid planId)
        {
            var modules =
                await _moduleRepo.GetModulesByPlanIdAsync(planId);

            var result = new
            {
                userId,
                planId,
                modules = new List<object>()
            };

            foreach (var m in modules)
            {
                var progress =
                    await _progressService.GetModuleProgressAsync(
                        userId, m.ModuleId);

                result.modules.Add(new
                {
                    moduleId = m.ModuleId,
                    isUnlocked = true, 
                    isCompleted = progress >= 100,
                    lessonProgressPercent = (int)progress
                });
            }

            return Ok(result);
        }

    }

}
