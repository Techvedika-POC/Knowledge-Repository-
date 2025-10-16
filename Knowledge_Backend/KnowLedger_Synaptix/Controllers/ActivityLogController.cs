using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Controllers
{
    /// <summary>
    /// Controller for managing user contributions — handles fetching,
    /// filtering, and retrieving contribution-related data for the logged-in user.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication for all endpoints
    public class ContributionsController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ContributionsController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyContributions()
        {
            // Get user ID from JWT claims
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized("User ID not found or invalid.");

            var contributions = await _activityLogService.GetUserContributionsAsync(userId.Value);
            return Ok(contributions);
        }

        [HttpGet("preview/{itemId}")]
        public async Task<IActionResult> GetContributionDetails(Guid itemId)
        {
            var item = await _activityLogService.GetContributionDetailsAsync(itemId);
            if (item == null)
                return NotFound("Knowledge item not found.");

            return Ok(item);
        }

        /// <summary>
        /// Retrieves contributions for the logged-in user with optional filters
        /// such as domain, category, title, status, and date.
        /// </summary>
        [HttpGet("my/filter")]
        public async Task<IActionResult> GetMyContributionsFiltered(
            [FromQuery] string domain = null,
            [FromQuery] string category = null,
            [FromQuery] string title = null,
            [FromQuery] string status = null,
            [FromQuery] string date = null)
        {
            // Parse optional date filter
            DateTime? parsedDate = null;
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var dt))
                parsedDate = dt;

            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized("User ID not found or invalid.");

            var contributions = await _activityLogService.GetUserContributionsFilteredAsync(
                userId.Value, domain, category, title, status, parsedDate);

            // Return an empty list instead of null for consistency
            return Ok(contributions?.Any() == true ? contributions : new List<ActivityLogDto>());
        }

        [HttpGet("my/domains")]
        public async Task<IActionResult> GetUserDomains()
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized("User ID not found or invalid.");

            var domains = await _activityLogService.GetUserDomainsAsync(userId.Value);
            return Ok(domains);
        }

        [HttpGet("my/categories")]
        public async Task<IActionResult> GetUserCategories()
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized("User ID not found or invalid.");

            var categories = await _activityLogService.GetUserCategoriesAsync(userId.Value);
            return Ok(categories);
        }

        [HttpGet("my/titles")]
        public async Task<IActionResult> GetUserTitles()
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized("User ID not found or invalid.");

            var titles = await _activityLogService.GetUserTitlesAsync(userId.Value);
            return Ok(titles);
        }

        // Extracts and validates the authenticated user's ID from JWT claims
        private Guid? GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
