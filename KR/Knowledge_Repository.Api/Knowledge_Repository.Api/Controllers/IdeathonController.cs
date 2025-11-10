using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdeathonController : ControllerBase
    {
        private readonly IIdeathonService _ideathonService;

        public IdeathonController(IIdeathonService ideathonService)
        {
            _ideathonService = ideathonService;
        }

        // ========================================================
        // TEAM MANAGEMENT
        // ========================================================
        [HttpPost("{eventId:guid}/register-team")]
        public async Task<IActionResult> RegisterTeam(Guid eventId, [FromBody] IdeathonTeamRegistrationDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TeamName) || dto.MemberIds == null || dto.MemberIds.Count == 0)
                return BadRequest(new { success = false, message = "Invalid team registration data." });

            // Implement later when team registration is supported
            return StatusCode(501, new { success = false, message = "Team registration not implemented yet." });
        }

        [HttpGet("{eventId:guid}/teams")]
        public async Task<IActionResult> GetTeamsByEvent(Guid eventId)
        {
            if (eventId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            var teams = await _ideathonService.GetTeamsForEventAsync(eventId);
            return Ok(new { success = true, data = teams });
        }

        // ========================================================
        // MENTOR MANAGEMENT
        // ========================================================
        [HttpGet("{eventId:guid}/mentors")]
        public async Task<IActionResult> GetAvailableMentors(Guid eventId)
        {
            if (eventId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            try
            {
                var mentors = await _ideathonService.GetAvailableMentorsAsync(eventId);
                return Ok(new { success = true, data = mentors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to fetch mentors.", details = ex.Message });
            }
        }

        [HttpPost("{eventId:guid}/teams/{teamId:guid}/assign-mentor")]
        public async Task<IActionResult> AssignMentorToTeam(Guid eventId, Guid teamId, [FromBody] AssignMentorDto dto)
        {
            if (eventId == Guid.Empty || teamId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event or team ID." });

            if (dto == null || dto.MentorIds == null || dto.MentorIds.Count == 0)
                return BadRequest(new { success = false, message = "Invalid mentor data." });

            try
            {
                foreach (var mentorId in dto.MentorIds)
                    await _ideathonService.AssignMentorToTeamAsync(eventId, teamId, mentorId);

                return Ok(new { success = true, message = "Mentor(s) assigned successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to assign mentor(s).", details = ex.Message });
            }
        }

        [HttpDelete("{eventId:guid}/teams/{teamId:guid}/remove-mentor/{mentorId:guid}")]
        public async Task<IActionResult> RemoveMentorFromTeam(Guid eventId, Guid teamId, Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid mentor ID." });

            try
            {
                await _ideathonService.RemoveMentorFromTeamAsync(mentorId);
                return Ok(new { success = true, message = "Mentor removed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to remove mentor.", details = ex.Message });
            }
        }

        // ========================================================
        // JURY MANAGEMENT
        // ========================================================
        [HttpGet("{eventId:guid}/jury")]
        public async Task<IActionResult> GetJuryByEvent(Guid eventId)
        {
            if (eventId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            try
            {
                var juryMembers = await _ideathonService.GetJuryMembersForEventAsync(eventId);
                return Ok(new { success = true, data = juryMembers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to fetch jury members.", details = ex.Message });
            }
        }

        [HttpPost("{eventId:guid}/jury")]
        public async Task<IActionResult> CreateJury(Guid eventId, [FromBody] CreateJuryDto dto)
        {
            if (eventId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            if (dto == null || dto.UserIds == null || dto.UserIds.Count == 0)
                return BadRequest(new { success = false, message = "Invalid jury data." });

            try
            {
                await _ideathonService.AssignJuryMembersToEventAsync(eventId, dto.UserIds);
                return Ok(new { success = true, message = "Jury members added successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding jury members.", details = ex.Message });
            }
        }

        [HttpDelete("{eventId:guid}/jury/{userId:guid}")]
        public async Task<IActionResult> RemoveJuryMember(Guid eventId, Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid jury member ID." });

            try
            {
                await _ideathonService.RemoveJuryMemberFromEventAsync(eventId, userId);
                return Ok(new { success = true, message = "Jury member removed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to remove jury member.", details = ex.Message });
            }
        }

        // ========================================================
        // PRESENTATION SCHEDULING
        // ========================================================
        [HttpGet("{eventId:guid}/presentations")]
        public async Task<IActionResult> GetPresentationsByEvent(Guid eventId)
        {
            if (eventId == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid event ID." });

            try
            {
                var presentations = await _ideathonService.GetPresentationsForEventAsync(eventId);
                return Ok(new { success = true, data = presentations });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to fetch presentations.", details = ex.Message });
            }
        }

        [HttpPost("{eventId:guid}/teams/{teamId:guid}/schedule-presentation")]
        public async Task<IActionResult> SchedulePresentation(Guid eventId, Guid teamId, [FromBody] SchedulePresentationDto dto)
        {
            if (dto == null || dto.PresentationDate == default)
                return BadRequest(new { success = false, message = "Invalid presentation data." });

            try
            {
                await _ideathonService.SchedulePresentationAsync(eventId, teamId, dto.PresentationDate);
                return Ok(new { success = true, message = "Presentation scheduled successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to schedule presentation.", details = ex.Message });
            }
        }
    }
}
