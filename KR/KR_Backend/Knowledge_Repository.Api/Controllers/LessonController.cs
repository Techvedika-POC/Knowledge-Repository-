using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet("{lessonId}")]
        public async Task<ActionResult<LessonDto>> GetLesson(Guid lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
            if (lesson == null) return NotFound();
            return Ok(lesson);
        }

        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessonsByModule(Guid moduleId)
        {
            var lessons = await _lessonService.GetLessonsByModuleIdAsync(moduleId);
            return Ok(lessons);
        }

        [HttpPost]
        public async Task<ActionResult<LessonDto>> CreateLesson([FromBody] LessonDto dto)
        {
            var created = await _lessonService.CreateLessonAsync(dto);
            return Ok(created);
        }

        [HttpPost("batch")]
        public async Task<ActionResult<IEnumerable<LessonDto>>> CreateLessonsBatch([FromBody] IEnumerable<LessonDto> lessons)
        {
            var created = await _lessonService.CreateLessonsBatchAsync(lessons);
            return Ok(created);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLesson([FromBody] LessonDto lesson)
        {
            await _lessonService.UpdateLessonAsync(lesson);
            return NoContent();
        }

        [HttpDelete("{lessonId}")]
        public async Task<IActionResult> DeleteLesson(Guid lessonId)
        {
            await _lessonService.DeleteLessonAsync(lessonId);
            return NoContent();
        }

        [HttpPost("{lessonId}/complete/{userId}")]
        public async Task<IActionResult> MarkCompleted(Guid lessonId, Guid userId)
        {
            await _lessonService.MarkLessonCompletedAsync(lessonId, userId);
            return NoContent();
        }
    }
}
