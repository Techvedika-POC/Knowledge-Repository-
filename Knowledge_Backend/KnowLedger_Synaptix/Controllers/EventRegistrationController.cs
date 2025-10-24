using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    /// <summary>
    /// Controller for managing event registrations and team management.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize] // Only authenticated users can register for events
    [ApiController]
    public class EventRegistrationController : ControllerBase
    {
        private readonly IEventRegistrationService _eventService;

        public EventRegistrationController(IEventRegistrationService eventService)
        {
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        /// <summary>
        /// Registers a new team for a specific event.
        /// </summary>
        [HttpPost("register-team")]
        public async Task<IActionResult> RegisterTeam([FromBody] EventRegistrationDto dto)
        {
            try
            {
                // Get logged-in user's ID from JWT claims
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

        /// <summary>
        /// Checks if the logged-in user is already registered for a specific event.
        /// </summary>
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
