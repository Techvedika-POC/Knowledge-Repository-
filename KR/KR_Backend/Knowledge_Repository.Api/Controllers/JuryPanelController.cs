// File: Knowledge_Repository.Controllers/JuryPanelController.cs
using Knowledge_Repository.Application.Dtos.JuryCommunication;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JuryPanelController : ControllerBase
    {
        private readonly IJuryPanelService _panelService;
        private readonly IJuryChatService _chatService;
        private readonly ILogger<JuryPanelController> _logger;

        public JuryPanelController(
            IJuryPanelService panelService,
            IJuryChatService chatService,
            ILogger<JuryPanelController> logger)
        {
            _panelService = panelService ?? throw new ArgumentNullException(nameof(panelService));
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("Teams/{eventId}")]
        public async Task<IActionResult> GetTeamsForEvent(Guid eventId)
        {
            var teams = await _panelService.GetTeamsWithMembersByEventAsync(eventId);
            return Ok(teams);
        }
        [HttpPost("FinalScore")]
        public async Task<IActionResult> SubmitFinalScore([FromBody] FinalScoreDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var id = await _panelService.SubmitFinalScoreAsync(dto);
                return Ok(new { FinalScoreId = id });
            }
            catch (InvalidOperationException invEx)
            {
                if (invEx.Message?.Contains("already submitted") == true)
                    return Conflict(new { error = invEx.Message });

                return BadRequest(new { error = invEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting final score for event {EventId} team {TeamId}", dto.EventId, dto.TeamId);
                return Problem(detail: "Internal server error", title: "Failed to submit final score");
            }
        }

        [HttpGet("JuryChat/{eventId}")]
        public async Task<IActionResult> GetJuryChat(Guid eventId)
        {
            var messages = await _chatService.GetMessagesAsync(eventId);
            return Ok(messages);
        }

        [HttpPost("JuryChat")]
        public async Task<IActionResult> PostJuryChat([FromBody] CreateJuryChatMessageDto dto)
        {
            var id = await _chatService.SendMessageAsync(dto);
            return Ok(new { MessageId = id });
        }
    }
}
