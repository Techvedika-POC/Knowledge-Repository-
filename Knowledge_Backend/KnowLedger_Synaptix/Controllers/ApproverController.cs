using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowLedger_Synaptix.Controllers
{
    [Route("api/approver")]
    [ApiController]
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
            var items = await _approverService.GetPendingKnowledgeItemsAsync();
            return Ok(items);
        }

        // POST: api/approver/approve/{itemId}
        [HttpPost("approve/{itemId}")]
        public async Task<IActionResult> ApproveItem(Guid itemId)
        {
            var approverId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? throw new Exception("UserId not found"));

            var result = await _approverService.ApproveKnowledgeItemAsync(itemId, approverId);
            if (!result) return BadRequest("Item cannot be approved.");
            return Ok("Item approved successfully.");
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
