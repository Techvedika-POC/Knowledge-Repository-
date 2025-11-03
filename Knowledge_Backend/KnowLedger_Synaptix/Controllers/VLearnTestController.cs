using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KnowLedger_Synaptix.Controllers
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
