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
            var likesCount = await _service.GetLikesCountAsync(itemId);
            return Ok(new { message = "Item liked.", likesCount });
        }

        [HttpDelete("like/{itemId}")]
        public async Task<IActionResult> UnlikeItem(Guid itemId, [FromQuery] Guid userId)
        {
            await _service.RemoveEngagementAsync(itemId, userId, "Like");
            var likesCount = await _service.GetLikesCountAsync(itemId);
            return Ok(new { message = "Like removed.", likesCount });
        }


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

        [HttpDelete("favourite/{itemId}")]
        public async Task<IActionResult> UnfavouriteItem(Guid itemId, [FromQuery] Guid userId)
        {
            await _service.RemoveEngagementAsync(itemId, userId, "Favourite");
            return Ok(new { message = "Favourite removed." });
        }

        public class CommentRequestDto
        {
            public string CommentText { get; set; } = string.Empty;
            public Guid? ParentCommentId { get; set; } 
        }

        [HttpPost("comment/{itemId}")]
        public async Task<IActionResult> AddComment(Guid itemId, [FromQuery] Guid userId, [FromBody] CommentRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CommentText))
                return BadRequest("Comment cannot be empty.");

            var commentDto = new CommentDto
            {
                ItemId = itemId,
                UserId = userId,
                CommentText = dto.CommentText,
                ParentCommentId = dto.ParentCommentId
            };

            await _service.AddCommentAsync(commentDto);
            return Ok(new { message = "Comment added successfully." });
        }
 
        [HttpPut("comment/{engagementId}")]
        public async Task<IActionResult> UpdateComment(Guid engagementId, [FromBody] CommentRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CommentText))
                return BadRequest("Comment cannot be empty.");

            await _service.UpdateCommentAsync(engagementId, dto.CommentText);
            return Ok(new { message = "Comment updated successfully." });
        }

        [HttpGet("comments/{itemId}")]
        public async Task<IActionResult> GetComments(Guid itemId)
        {
            var comments = await _service.GetCommentsByItemAsync(itemId);
            return Ok(comments);
        }

        [HttpGet("replies/{parentCommentId}")]
        public async Task<IActionResult> GetReplies(Guid parentCommentId)
        {
            var replies = await _service.GetRepliesAsync(parentCommentId);
            return Ok(replies);
        }


        [HttpDelete("comment/{engagementId}")]
        public async Task<IActionResult> DeleteComment(Guid engagementId)
        {
            await _service.DeleteCommentAsync(engagementId);
            return Ok(new { message = "Comment deleted successfully." });
        }


        [HttpGet("summary/{itemId}")]
        public async Task<IActionResult> GetSummary(Guid itemId, [FromQuery] Guid userId)
        {
            var summary = await _service.GetEngagementSummaryAsync(itemId, userId);
            return Ok(summary);
        }

        [HttpGet("user-engagements/{userId}")]
        public async Task<IActionResult> GetUserEngagements(Guid userId)
        {
            var engagements = await _service.GetUserEngagementsAsync(userId);
            return Ok(engagements);
        }

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
