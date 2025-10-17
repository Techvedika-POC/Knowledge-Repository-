using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowLedger_Synaptix.Controllers
{
    /// <summary>
    /// Handles approval and rejection of knowledge items by authorized approvers.
    /// </summary>
    [Route("api/approver")]
    [ApiController]
    [Authorize] // Only authenticated users (approvers) can access these endpoints
    public class ApproverController : ControllerBase
    {
        private readonly IApproverService _approverService;

        public ApproverController(IApproverService approverService)
        {
            _approverService = approverService;
        }

        // GET: api/approver/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingItems()
        {
            // Fetch all knowledge items currently waiting for approval
            var items = await _approverService.GetPendingKnowledgeItemsAsync();
            return Ok(items);
        }

        [HttpPost("approve/{itemId}")]
        public async Task<IActionResult> ApproveItem(Guid itemId)
        {
            // Get the approver ID from the logged-in user's claims
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
        public async Task<IActionResult> GetPendingPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var (items, totalCount) = await _approverService.GetPendingKnowledgeItemsAsync(pageNumber, pageSize);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new
            {
                items,
                totalCount,
                totalPages,
                pageNumber,
                pageSize
            });
        }



        // POST: api/approver/reject/{itemId}
        [HttpPost("reject/{itemId}")]
        public async Task<IActionResult> RejectItem(Guid itemId)
        {
            // Get logged-in user id (approver) from JWT or claims
            var approverId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? throw new Exception("UserId not found"));

            var result = await _approverService.RejectKnowledgeItemAsync(itemId, approverId);
            if (!result) return BadRequest("Item cannot be rejected.");
            return Ok("Item rejected successfully.");
        }
    }
}
