using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Knowledge_Repository.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VLearnTestController : ControllerBase
    {
        private readonly IVLearnTestService _testService;

        public VLearnTestController(IVLearnTestService testService)
        {
            _testService = testService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateQuestions([FromBody] VLearnQuestionRequestDto request)
        {
            if (string.IsNullOrEmpty(request.ModuleName))
                return BadRequest("Module name is required");

            var result = await _testService.GenerateQuestionsAsync(request);
            return Ok(result);
        }
    }
}
