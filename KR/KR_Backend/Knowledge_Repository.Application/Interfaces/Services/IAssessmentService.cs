using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<AssessmentDto?> GetAssessmentByIdAsync(Guid assessmentId);
        Task<IEnumerable<AssessmentDto>> GetAssessmentsByModuleIdAsync(Guid moduleId);
        Task<AssessmentDto> CreateAssessmentAsync(AssessmentDto dto);
        Task<IEnumerable<AssessmentDto>> CreateAssessmentsBatchAsync(IEnumerable<AssessmentDto> dtos);
        Task UpdateAssessmentAsync(AssessmentDto dto);
        Task DeleteAssessmentAsync(Guid assessmentId);
        Task AddQuestionsAsync(Guid assessmentId, IEnumerable<AssessmentQuestionDto> questions);
        Task UpdateQuestionAsync(AssessmentQuestionDto dto);
        Task StartAssessmentAsync(Guid assessmentId, Guid userId);
        Task DeleteQuestionAsync(Guid questionId);
        Task<bool> IsAssessmentUnlockedAsync(Guid assessmentId, Guid userId);
        Task<AssessmentResultDto> SubmitAssessmentAsync(SubmitAssessmentDto dto);
        Task<AssessmentResultDto?> GetUserAssessmentResultAsync(Guid assessmentId, Guid userId);
    }
}
