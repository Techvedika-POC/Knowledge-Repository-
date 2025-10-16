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
            _approverService = approverService ?? throw new ArgumentNullException(nameof(approverService));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingItems()
        {
            // Fetch all knowledge items currently waiting for approval
            var items = await _approverService.GetPendingKnowledgeItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Approves a specific knowledge item. 
        /// The approver's ID is extracted from the JWT claims for tracking.
        /// </summary>
        [HttpPost("approve/{itemId}")]
        public async Task<IActionResult> ApproveItem(Guid itemId)
        {
            // Get approver ID from JWT claims
            var approverIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(approverIdClaim))
                return Unauthorized("User ID not found in token.");

            // Validate and parse approver ID
            if (!Guid.TryParse(approverIdClaim, out var approverId))
                return BadRequest("Invalid user ID format.");

            var result = await _approverService.ApproveKnowledgeItemAsync(itemId, approverId);

            if (!result)
                return BadRequest("Item cannot be approved. It may already be processed or invalid.");

            return Ok("Item approved successfully.");
        }

        /// <summary>
        /// Rejects a specific knowledge item. 
        /// The approver ID is retrieved from the JWT for audit tracking.
        /// </summary>
        [HttpPost("reject/{itemId}")]
        public async Task<IActionResult> RejectItem(Guid itemId)
        {
            // Get approver ID from JWT claims
            var approverIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(approverIdClaim))
                return Unauthorized("User ID not found in token.");

            // Validate and parse approver ID
            if (!Guid.TryParse(approverIdClaim, out var approverId))
                return BadRequest("Invalid user ID format.");

            var result = await _approverService.RejectKnowledgeItemAsync(itemId, approverId);

            if (!result)
                return BadRequest("Item cannot be rejected. It may already be processed or invalid.");

            return Ok("Item rejected successfully.");
        }
    }
}
