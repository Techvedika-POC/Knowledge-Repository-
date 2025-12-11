using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;

        public AssessmentController(IAssessmentService service)
        {
            _assessmentService = service;
        }

        // Get Assessment + Questions
        [HttpGet("{assessmentId}")]
        public async Task<ActionResult<AssessmentDto>> GetAssessment(Guid assessmentId)
        {
            var data = await _assessmentService.GetAssessmentByIdAsync(assessmentId);
            return data == null ? NotFound() : Ok(data);
        }

        // List assessments of a module
        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> List(Guid moduleId)
        {
            return Ok(await _assessmentService.GetAssessmentsByModuleIdAsync(moduleId));
        }

        // Create assessment
        [HttpPost]
        public async Task<ActionResult<AssessmentDto>> Create(AssessmentDto dto)
        {
            return Ok(await _assessmentService.CreateAssessmentAsync(dto));
        }

        // Add questions
        [HttpPost("{assessmentId}/questions")]
        public async Task<IActionResult> AddQuestions(Guid assessmentId, IEnumerable<AssessmentQuestionDto> questions)
        {
            await _assessmentService.AddQuestionsAsync(assessmentId, questions);
            return NoContent();
        }

        // Update question
        [HttpPut("question/{questionId}")]
        public async Task<IActionResult> UpdateQuestion(Guid questionId, AssessmentQuestionDto dto)
        {
            dto.QuestionId = questionId;
            await _assessmentService.UpdateQuestionAsync(dto);
            return NoContent();
        }

        // Delete question
        [HttpDelete("question/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(Guid questionId)
        {
            await _assessmentService.DeleteQuestionAsync(questionId);
            return NoContent();
        }

        // ⭐ Save test result
        [HttpPost("submit")]
        public async Task<ActionResult<AssessmentResultDto>> SubmitTest(SubmitAssessmentDto dto)
        {
            var result = await _assessmentService.SubmitAssessmentAsync(dto);
            return Ok(result);
        }

        // ⭐ Get past result
        [HttpGet("{assessmentId}/user/{userId}/result")]
        public async Task<ActionResult<AssessmentResultDto>> GetUserResult(Guid assessmentId, Guid userId)
        {
            return Ok(await _assessmentService.GetUserAssessmentResultAsync(assessmentId, userId));
        }
    }
}
