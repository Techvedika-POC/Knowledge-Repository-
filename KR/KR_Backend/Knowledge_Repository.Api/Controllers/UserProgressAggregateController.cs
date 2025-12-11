using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProgressAggregateController : ControllerBase
    {
        private readonly IUserProgressAggregateService _service;

        public UserProgressAggregateController(IUserProgressAggregateService service)
        {
            _service = service;
        }

        [HttpGet("{userId}/plan/{planId}")]
        public async Task<ActionResult<UserPlanProgressDetailDto>> GetUserPlanProgress(Guid userId, Guid planId)
        {
            return Ok(await _service.GetUserPlanProgressAsync(userId, planId));
        }
    }

}
