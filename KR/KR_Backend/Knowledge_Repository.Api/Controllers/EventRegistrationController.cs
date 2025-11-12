using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
 
    [Route("api/[controller]")]
    [Authorize] 
    [ApiController]
    public class EventRegistrationController : ControllerBase
    {
        private readonly IEventRegistrationService _eventService;

        public EventRegistrationController(IEventRegistrationService eventService)
        {
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

    
        [HttpPost("register-team")]
        public async Task<IActionResult> RegisterTeam([FromBody] EventRegistrationDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User not logged in.");

                var userId = Guid.Parse(userIdClaim);

                var team = await _eventService.RegisterTeamForEventAsync(dto, userId);
                return Ok(new { success = true, team });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

 
        [HttpGet("is-registered/{eventId}")]
        public async Task<IActionResult> IsUserRegistered(Guid eventId)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User not logged in.");

                var userId = Guid.Parse(userIdClaim);

                bool isRegistered = await _eventService.IsUserAlreadyRegisteredAsync(userId, eventId);
                return Ok(new { success = true, isRegistered });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
