using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Dtos.CommunicationBetweenMentorAndTeam;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunicationController : ControllerBase
    {
        private readonly ICommunicationService _service;
        public CommunicationController(ICommunicationService service) { _service = service; }

        // GET chat messages for team
        [HttpGet("team/{teamId:guid}/chat")]
        public async Task<IActionResult> GetTeamChat(Guid teamId, [FromQuery] Guid? userId)
        {
           
            var callerId = GetCallerUserId(userId);
            if (callerId == Guid.Empty) return BadRequest("Invalid user.");

            var msgs = await _service.GetTeamChatMessagesAsync(callerId, teamId);
            return Ok(msgs);
        }

        // POST chat message
        [HttpPost("team/{teamId:guid}/chat")]
        public async Task<IActionResult> PostTeamChat(Guid teamId, [FromBody] ChatPostRequest req, [FromQuery] Guid? userId)
        {
            var callerId = GetCallerUserId(userId);
            if (callerId == Guid.Empty) return BadRequest("Invalid user.");
            if (string.IsNullOrWhiteSpace(req.MessageText)) return BadRequest("Message cannot be empty.");

            var msg = await _service.PostTeamChatMessageAsync(callerId, teamId, req.MessageText, req.SenderName);
            return CreatedAtAction(nameof(GetTeamChat), new { teamId }, msg);
        }

        // delete chat message
        [HttpDelete("chat/{messageId:guid}")]
        public async Task<IActionResult> DeleteChatMessage(Guid messageId, [FromQuery] Guid? userId)
        {
            var callerId = GetCallerUserId(userId);
            if (callerId == Guid.Empty) return BadRequest("Invalid user.");
            try
            {
                var ok = await _service.SoftDeleteChatMessageAsync(callerId, messageId);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch (System.Security.SecurityException se) { return Forbid(se.Message); }
        }

        // Feedback endpoints
        [HttpGet("team/{teamId:guid}/feedbacks")]
        public async Task<IActionResult> GetTeamFeedbacks(Guid teamId, [FromQuery] Guid? userId)
        {
            var callerId = GetCallerUserId(userId);
            if (callerId == Guid.Empty) return BadRequest("Invalid user.");
            var fbs = await _service.GetTeamFeedbacksAsync(callerId, teamId);
            return Ok(fbs);
        }
        [HttpPost("feedback")]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackCreateRequest req, [FromQuery] Guid? userId)
        {
            if (req == null) return BadRequest();
            req.RequestingUserId = GetCallerUserId(userId);

            try
            {
                var fb = await _service.CreateFeedbackAsync(req);
                return CreatedAtAction(nameof(GetTeamFeedbacks), new { teamId = fb.TeamId }, fb);
            }
            catch (System.Security.SecurityException se)
            {
                return StatusCode(403, new { message = se.Message });
            }
            catch (ArgumentException aex)
            {
                return BadRequest(new { message = aex.Message });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }





        [HttpPost("feedback/{feedbackId:guid}/reply")]
        public async Task<IActionResult> ReplyToFeedback(Guid feedbackId, [FromBody] FeedbackReplyCreateRequest req)
        {
            if (req == null) return BadRequest();
            if (req.FeedbackId != feedbackId) return BadRequest("FeedbackId mismatch.");

            try
            {
                var r = await _service.CreateFeedbackReplyAsync(req);
                return CreatedAtAction(nameof(GetTeamFeedbacks), new { teamId = r.TeamId }, r);
            }
            catch (System.Security.SecurityException se) { return Forbid(se.Message); }
        }

        private Guid GetCallerUserId(Guid? fallback)
        {
            
            try
            {
                var claim = User?.FindFirst("sub")?.Value ?? User?.FindFirst("userid")?.Value;
                if (!string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out var parsed)) return parsed;
            }
            catch {}

            if (fallback.HasValue && fallback.Value != Guid.Empty) return fallback.Value;
            return Guid.Empty;
        }
    }
}
