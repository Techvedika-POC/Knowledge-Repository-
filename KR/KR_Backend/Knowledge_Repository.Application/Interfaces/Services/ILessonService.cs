using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ILessonService
    {
        Task<LessonDto> GetLessonByIdAsync(Guid lessonId);
        Task<IEnumerable<LessonDto>> GetLessonsByModuleIdAsync(Guid moduleId);
        Task<LessonDto> CreateLessonAsync(LessonDto lessonDto);
        Task<IEnumerable<LessonDto>> CreateLessonsBatchAsync(IEnumerable<LessonDto> lessons);
        Task UpdateLessonAsync(LessonDto lessonDto);
        Task DeleteLessonAsync(Guid lessonId);
        Task MarkLessonCompletedAsync(Guid lessonId, Guid userId);
    }
}
