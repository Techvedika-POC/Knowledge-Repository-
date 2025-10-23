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
    [Authorize] 
    public class ContributionsController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ContributionsController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

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
                return Ok(new List<ActivityLogDto>()); 

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
        [HttpGet("my/paged")]
        public async Task<IActionResult> GetMyContributionsPaged(
          [FromQuery] int pageNumber = 1,
          [FromQuery] int pageSize = 10,
          [FromQuery] string domain = null,
          [FromQuery] string category = null,
          [FromQuery] string title = null,
          [FromQuery] string status = null,
          [FromQuery] DateTime? date = null)
        {
           
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user.");

            var pagedResult = await _activityLogService.GetUserContributionsPagedAsync(
                userId, pageNumber, pageSize, domain, category, title, status, date);

            return Ok(pagedResult);
        }
        [HttpGet("user/contributions/month")]
        public async Task<IActionResult> GetUserContributionsThisMonth()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var contributions = await _activityLogService.GetUserContributionsThisMonthAsync(userId);
            return Ok(contributions);
        }
        [HttpGet("my/favourites")]
        public async Task<IActionResult> GetMyFavourites()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var items = await _activityLogService.GetUserFavouritesAsync(userId);
            return Ok(items);
        }
    }
}
