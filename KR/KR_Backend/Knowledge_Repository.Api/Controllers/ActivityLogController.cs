using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Knowledge_Repository.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ContributionsController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ContributionsController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst("nameid")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
        [HttpGet("my")]
        public async Task<IActionResult> GetMyContributions()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized("Invalid user.");

            var contributions = await _activityLogService.GetUserContributionsAsync(userId);
            return Ok(contributions ?? new List<ActivityLogDto>());
        }

        [HttpGet("user/contributions/month")]
        public async Task<IActionResult> GetMyContributionsThisMonth()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized("Invalid user.");

            var contributions = await _activityLogService.GetUserContributionsThisMonthAsync(userId);
            return Ok(contributions ?? new List<ActivityLogDto>());
        }

        [HttpGet("preview/{itemId}")]
        public async Task<IActionResult> GetContributionDetails(Guid itemId)
        {
            var item = await _activityLogService.GetContributionDetailsAsync(itemId);
            if (item == null) return NotFound("Knowledge item not found.");
            return Ok(item);
        }
        [HttpGet("my/favourites")]
        public async Task<IActionResult> GetMyFavourites()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized("Invalid user.");

            var favourites = await _activityLogService.GetUserFavouritesAsync(userId);
            return Ok(favourites ?? new List<KnowledgeItemDto>());
        }

        [HttpGet("my/filter")]
        public async Task<IActionResult> GetMyContributionsFiltered(
            [FromQuery] string domain = null,
            [FromQuery] string category = null,
            [FromQuery] string title = null,
            [FromQuery] string status = null,
            [FromQuery] string date = null)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized("Invalid user.");

            DateTime? parsedDate = null;
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var dt))
                parsedDate = dt;

            var contributions = await _activityLogService.GetUserContributionsFilteredAsync(
                userId, domain, category, title, status, parsedDate);

            return Ok(contributions ?? new List<ActivityLogDto>());
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
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized("Invalid user.");

            var pagedResult = await _activityLogService.GetUserContributionsPagedAsync(
                userId, pageNumber, pageSize, domain, category, title, status, date);

            return Ok(pagedResult ?? new PagedResult<ActivityLogDto>
            {
                Items = new List<ActivityLogDto>(),
                TotalCount = 0
            });
        }

        [HttpGet("my/domains")]
        public async Task<IActionResult> GetUserDomains()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized("Invalid user.");

            var domains = await _activityLogService.GetUserDomainsAsync(userId);
            return Ok(domains ?? new List<string>());
        }

        [HttpGet("my/categories")]
        public async Task<IActionResult> GetUserCategories()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized("Invalid user.");

            var categories = await _activityLogService.GetUserCategoriesAsync(userId);
            return Ok(categories ?? new List<string>());
        }

        [HttpGet("my/titles")]
        public async Task<IActionResult> GetUserTitles()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized("Invalid user.");

            var titles = await _activityLogService.GetUserTitlesAsync(userId);
            return Ok(titles ?? new List<string>());
        }
    }
}
