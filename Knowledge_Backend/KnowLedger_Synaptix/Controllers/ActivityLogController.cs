using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ContributionsController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ContributionsController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        //// GET: api/contributions/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyContributions()
        {
            // Fetch logged-in user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID.");

            var contributions = await _activityLogService.GetUserContributionsAsync(userId);

            return Ok(contributions);
        }


        // GET: api/contributions/preview/{itemId}
        [HttpGet("preview/{itemId}")]
        public async Task<IActionResult> GetContributionDetails(Guid itemId)
        {
            var item = await _activityLogService.GetContributionDetailsAsync(itemId);
            if (item == null)
                return NotFound("Knowledge item not found.");

            return Ok(item);
        }
        [HttpGet("my/filter")]
        public async Task<IActionResult> GetMyContributionsFiltered(
    [FromQuery] string domain = null,
    [FromQuery] string category = null,
    [FromQuery] string title = null,
    [FromQuery] string status = null,
    [FromQuery] string date = null)
        {
            DateTime? parsedDate = null;
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var dt))
                parsedDate = dt;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID.");

            var contributions = await _activityLogService.GetUserContributionsFilteredAsync(
                userId, domain, category, title, status, parsedDate);

            if (contributions == null || !contributions.Any())
                return Ok(new List<ActivityLogDto>()); // return empty list instead of error

            return Ok(contributions);
        }


        [HttpGet("my/domains")]
        public async Task<IActionResult> GetUserDomains()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID.");

            var domains = await _activityLogService.GetUserDomainsAsync(userId);
            return Ok(domains);
        }

        [HttpGet("my/categories")]
        public async Task<IActionResult> GetUserCategories()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID.");

            var categories = await _activityLogService.GetUserCategoriesAsync(userId);
            return Ok(categories);
        }

        [HttpGet("my/titles")]
        public async Task<IActionResult> GetUserTitles()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID.");

            var titles = await _activityLogService.GetUserTitlesAsync(userId);
            return Ok(titles);
        }
    }
}
