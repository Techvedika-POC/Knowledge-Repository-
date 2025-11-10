using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EngagementController : ControllerBase
    {
        private readonly IEngagementService _service;

        public EngagementController(IEngagementService service)
        {
            _service = service;
        }

        /// <summary>
        /// Like an item.
        /// </summary>
        [HttpPost("like/{itemId}")]
        public async Task<IActionResult> LikeItem(Guid itemId, [FromQuery] Guid userId)
        {
            var dto = new EngagementDto
            {
                ItemId = itemId,
                UserId = userId,
                EngagementType = "Like"
            };

            await _service.AddEngagementAsync(dto);
            return Ok(new { message = "Item liked.", likesCount = await _service.GetLikesCountAsync(itemId) });
        }

        /// <summary>
        /// Favourite an item.
        /// </summary>
        [HttpPost("favourite/{itemId}")]
        public async Task<IActionResult> FavouriteItem(Guid itemId, [FromQuery] Guid userId)
        {
            var dto = new EngagementDto
            {
                ItemId = itemId,
                UserId = userId,
                EngagementType = "Favourite"
            };

            await _service.AddEngagementAsync(dto);
            return Ok(new { message = "Item favourited." });
        }

        public class CommentRequestDto
        {
            public string CommentText { get; set; } = string.Empty;
        }

        /// <summary>
        /// Add a comment to an item.
        /// </summary>
        [HttpPost("comment/{itemId}")]
        public async Task<IActionResult> CommentItem(Guid itemId, [FromQuery] Guid userId, [FromBody] CommentRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CommentText))
                return BadRequest("Comment cannot be empty.");

            var engagementDto = new EngagementDto
            {
                ItemId = itemId,
                UserId = userId,
                EngagementType = "Comment",
                CommentText = dto.CommentText
            };

            await _service.AddEngagementAsync(engagementDto);
            return Ok(new { message = "Comment added." });
        }

        /// <summary>
        /// Get summary of engagements for an item.
        /// </summary>
        [HttpGet("summary/{itemId}")]
        public async Task<IActionResult> GetSummary(Guid itemId, [FromQuery] Guid userId)
        {
            var summary = await _service.GetEngagementSummaryAsync(itemId, userId);
            return Ok(summary);
        }

        /// <summary>
        /// Get all engagements for a particular user.
        /// </summary>
        [HttpGet("user-engagements/{userId}")]
        public async Task<IActionResult> GetUserEngagements(Guid userId)
        {
            var engagements = await _service.GetUserEngagementsAsync(userId);
            return Ok(engagements);
        }

        /// <summary>
        /// Remove a like from an item.
        /// </summary>
        [HttpDelete("like/{itemId}")]
        public async Task<IActionResult> UnlikeItem(Guid itemId, [FromQuery] Guid userId)
        {
            await _service.RemoveEngagementAsync(itemId, userId, "Like");
            return Ok(new { message = "Like removed.", likesCount = await _service.GetLikesCountAsync(itemId) });
        }

        /// <summary>
        /// Remove a favourite from an item.
        /// </summary>
        [HttpDelete("favourite/{itemId}")]
        public async Task<IActionResult> UnfavouriteItem(Guid itemId, [FromQuery] Guid userId)
        {
            await _service.RemoveEngagementAsync(itemId, userId, "Favourite");
            return Ok(new { message = "Favourite removed." });
        }

        /// <summary>
        /// Get the top liked items.
        /// </summary>
        [HttpGet("top-liked")]
        public async Task<IActionResult> GetTopLikedItems([FromQuery] int top = 5)
        {
            try
            {
                var leaderboard = await _service.GetTopLikedItemsAsync(top);
                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve leaderboard", error = ex.Message });
            }
        }
    }
}
