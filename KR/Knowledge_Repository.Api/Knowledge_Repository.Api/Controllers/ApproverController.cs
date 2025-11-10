using Knowledge_Repository.Application.Dtos;
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
    [Authorize]
    public class ApproverController : ControllerBase
    {
        private readonly IApproverService _approverService;

        public ApproverController(IApproverService approverService)
        {
            _approverService = approverService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingItems()
        {
            var items = await _approverService.GetPendingKnowledgeItemsAsync();
            return Ok(items);
        }

        [HttpPost("approve/{itemId}")]
        public async Task<IActionResult> ApproveItem(Guid itemId)
        {
            var approverIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(approverIdClaim))
                return Unauthorized("User ID not found in token.");

            if (!Guid.TryParse(approverIdClaim, out var approverId))
                return BadRequest("Invalid user ID format.");

            var result = await _approverService.ApproveKnowledgeItemAsync(itemId, approverId);

            if (!result)
                return BadRequest("Item cannot be approved.");

            return Ok("Item approved successfully.");
        }

        [HttpGet("pending/paged")]
        public async Task<IActionResult> GetPendingPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (items, totalCount) = await _approverService.GetPendingKnowledgeItemsAsync(pageNumber, pageSize);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new
            {
                items,
                totalCount,
                totalPages,
                pageNumber,
                pageSize
            });
        }

        [HttpPost("reject/{itemId}")]
        public async Task<IActionResult> RejectItem(Guid itemId)
        {
            var approverIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(approverIdClaim))
                return Unauthorized("User ID not found in claims.");

            if (!Guid.TryParse(approverIdClaim, out var approverId))
                return BadRequest("Invalid user ID format.");

            var result = await _approverService.RejectKnowledgeItemAsync(itemId, approverId);

            if (!result)
                return BadRequest("Item cannot be rejected.");

            return Ok("Item rejected successfully.");
        }
    }
}
