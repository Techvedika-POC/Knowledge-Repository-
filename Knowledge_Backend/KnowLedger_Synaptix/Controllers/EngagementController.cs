using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        return Ok(new { message = "Item liked.", likesCount = await _service.GetLikesCountAsync(itemId) });
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
    public class CommentRequestDto
    {
        public string CommentText { get; set; } = string.Empty;
    }

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
    [HttpDelete("like/{itemId}")]
    public async Task<IActionResult> UnlikeItem(Guid itemId, [FromQuery] Guid userId)
    {
        await _service.RemoveEngagementAsync(itemId, userId, "Like");
        return Ok(new { message = "Like removed.", likesCount = await _service.GetLikesCountAsync(itemId) });
    }

    [HttpDelete("favourite/{itemId}")]
    public async Task<IActionResult> UnfavouriteItem(Guid itemId, [FromQuery] Guid userId)
    {
        await _service.RemoveEngagementAsync(itemId, userId, "Favourite");
        return Ok(new { message = "Favourite removed." });
    }



}
