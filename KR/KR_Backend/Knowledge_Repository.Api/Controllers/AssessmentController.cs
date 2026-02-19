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
        private readonly IUserProgressService _userProgressService;
        public AssessmentController(IAssessmentService service, IUserProgressService userProgressService)
        {
            _assessmentService = service;
            _userProgressService = userProgressService;
        }

        [HttpGet("{assessmentId}")]
        public async Task<ActionResult<AssessmentDto>> GetAssessment(Guid assessmentId)
        {
            var data = await _assessmentService.GetAssessmentByIdAsync(assessmentId);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> List(Guid moduleId)
        {
            return Ok(await _assessmentService.GetAssessmentsByModuleIdAsync(moduleId));
        }
        [HttpPost]
        public async Task<ActionResult<AssessmentDto>> Create(AssessmentDto dto)
        {
            return Ok(await _assessmentService.CreateAssessmentAsync(dto));
        }

        [HttpPost("{assessmentId}/questions")]
        public async Task<IActionResult> AddQuestions(Guid assessmentId, IEnumerable<AssessmentQuestionDto> questions)
        {
            await _assessmentService.AddQuestionsAsync(assessmentId, questions);
            return NoContent();
        }

        [HttpPut("question/{questionId}")]
        public async Task<IActionResult> UpdateQuestion(Guid questionId, AssessmentQuestionDto dto)
        {
            dto.QuestionId = questionId;
            await _assessmentService.UpdateQuestionAsync(dto);
            return NoContent();
        }

        [HttpDelete("question/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(Guid questionId)
        {
            await _assessmentService.DeleteQuestionAsync(questionId);
            return NoContent();
        }
        [HttpPost("{assessmentId}/start/{userId}")]
        public async Task<IActionResult> Start(Guid assessmentId, Guid userId)
        {
            await _assessmentService.StartAssessmentAsync(assessmentId, userId);
            return Ok();
        }
        [HttpPost("{assessmentId}/submit")]
        public async Task<IActionResult> Submit(
          Guid assessmentId,
          SubmitAssessmentDto dto)
        {
            dto.AssessmentId = assessmentId;

            var result = await _userProgressService
                .SubmitAssessmentAsync(dto);

            return Ok(result);
        }

        [HttpDelete("{assessmentId}")]
        public async Task<IActionResult> DeleteAssessment(Guid assessmentId)
        {
            await _assessmentService.DeleteAssessmentAsync(assessmentId);
            return NoContent();
        }
    }
}
