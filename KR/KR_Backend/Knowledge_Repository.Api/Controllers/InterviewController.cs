using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Api.Controllers
{
    [ApiController]
    [Route("api/interview")]
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewService _service;

        public InterviewController(IInterviewService service)
        {
            _service = service;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartInterview(StartInterviewDto dto)
        {
            var result = await _service.StartInterviewAsync(dto);
            return Ok(new { interviewId = result }); 
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage(
            [FromBody] InterviewMessageDto dto)
        {
            var result = await _service.SendMessageAsync(dto);
            return Ok(result);
        }
    }

}
