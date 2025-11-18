using Knowledge_Repository.Application.Dtos.Mentor;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MentorController : ControllerBase
    {
        private readonly IMentorService _mentorService;

        public MentorController(IMentorService mentorService)
        {
            _mentorService = mentorService;
        }

        [HttpGet("{mentorId}/teams")]
        public async Task<IActionResult> GetTeamsForMentor(Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                return BadRequest("Invalid mentor ID.");

            var teams = await _mentorService.GetTeamsForMentorAsync(mentorId);
            return Ok(teams);
        }


        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetTeamDetails(Guid teamId)
        {
            if (teamId == Guid.Empty)
                return BadRequest("Invalid team ID.");

            var teamDetails = await _mentorService.GetTeamDetailsAsync(teamId);
            if (teamDetails == null)
                return NotFound("Team not found.");

            return Ok(teamDetails);
        }

  
        [HttpPost("feedback/add")]
        public async Task<IActionResult> AddFeedback([FromBody] AddFeedbackRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            try
            {
                var response = await _mentorService.AddFeedbackAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpPut("feedback/update")]
        public async Task<IActionResult> UpdateFeedback([FromBody] UpdateFeedbackRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _mentorService.UpdateFeedbackAsync(request);
            if (!updated)
                return NotFound("Feedback not found.");

            return Ok("Feedback updated successfully.");
        }

        [HttpGet("{mentorId}/feedbacks")]
        public async Task<IActionResult> GetFeedbacksByMentor(Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                return BadRequest("Invalid mentor ID.");

            var feedbacks = await _mentorService.GetFeedbacksByMentorAsync(mentorId);
            return Ok(feedbacks);
        }
        [HttpGet("feedback/{feedbackId}")]
        public async Task<IActionResult> GetFeedbackById(Guid feedbackId)
        {
            if (feedbackId == Guid.Empty)
                return BadRequest("Invalid feedback ID.");

            var feedback = await _mentorService.GetFeedbackByIdAsync(feedbackId);
            if (feedback == null)
                return NotFound("Feedback not found.");

            return Ok(feedback);
        }
        [HttpGet("{mentorId}/teams/{eventId}")]
        public async Task<IActionResult> GetTeamsForMentorByEvent(Guid mentorId, Guid eventId)
        {
            if (mentorId == Guid.Empty || eventId == Guid.Empty)
                return BadRequest("Invalid mentor or event ID.");

            var teams = await _mentorService.GetTeamsForMentorByEventAsync(mentorId, eventId);
            return Ok(teams);
        }

    }
}
