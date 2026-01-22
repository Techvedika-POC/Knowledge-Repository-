using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Knowledge_Repository.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Approver,Admin")]
    public class ApproverController : ControllerBase
    {
        private readonly IApproverService _approverService;

        public ApproverController(IApproverService approverService)
        {
            _approverService = approverService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _approverService.GetDashboardSummaryAsync();
            return Ok(dashboard);
        }

        [HttpGet("pending/normal")]
        public async Task<IActionResult> GetNormalPending(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var (items, total) = await _approverService.GetNormalPendingAsync(page, size);
            var totalPages = (int)Math.Ceiling(total / (double)size);

            return Ok(new
            {
                items,
                total,
                totalPages,
                page,
                size
            });
        }


        [HttpGet("pending/event")]
        public async Task<IActionResult> GetEventPending(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var (items, total) = await _approverService.GetEventPendingAsync(page, size);
            var totalPages = (int)Math.Ceiling(total / (double)size);

            return Ok(new
            {
                items,
                total,
                totalPages,
                page,
                size
            });
        }

 
        [HttpGet("pending/event/{eventId}")]
        public async Task<IActionResult> GetPendingByEvent(
            Guid eventId,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var (items, total) = await _approverService.GetPendingByEventAsync(eventId, page, size);
            var totalPages = (int)Math.Ceiling(total / (double)size);

            return Ok(new
            {
                items,
                total,
                totalPages,
                eventId,
                page,
                size
            });
        }

 
        [HttpPost("approve/{itemId}")]
        public async Task<IActionResult> ApproveItem(Guid itemId)
        {
            var approverId = GetLoggedInUserId();
            if (approverId == null)
                return Unauthorized("User ID missing in token.");

            var success = await _approverService.ApproveAsync(itemId, approverId.Value);
            if (!success)
                return BadRequest("Unable to approve the item.");

            return Ok("Item approved successfully.");
        }
        [HttpPost("reject/{itemId}")]
        public async Task<IActionResult> RejectItem(
            Guid itemId,
            [FromBody] RejectDto dto)
        {
            var approverId = GetLoggedInUserId();
            if (approverId == null)
                return Unauthorized("User ID missing in token.");

            if (dto == null || string.IsNullOrWhiteSpace(dto.Feedback))
                return BadRequest("Feedback is required.");

            await _approverService
                .RejectAsync(itemId, approverId.Value, dto.Feedback);

            return Ok("Item rejected successfully.");
        }



        private Guid? GetLoggedInUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        [HttpGet("event/types")]
        public async Task<IActionResult> GetEventTypes()
        {
            var types = await _approverService.GetEventTypesAsync();
            return Ok(types);
        }
        [HttpGet("pending/event/type/{eventType}")]
        public async Task<IActionResult> GetPendingByEventType(
            string eventType,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var (items, total) = await _approverService.GetPendingByEventTypeAsync(eventType, page, size);

            var totalPages = (int)Math.Ceiling(total / (double)size);

            return Ok(new
            {
                items,
                total,
                totalPages,
                eventType,
                page,
                size
            });
        }

    }
}
