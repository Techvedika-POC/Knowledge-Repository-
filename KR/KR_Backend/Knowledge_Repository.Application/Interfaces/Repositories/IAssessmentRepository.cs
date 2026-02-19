using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IAssessmentRepository : IGenericRepository<Assessment>
    {
        Task<IEnumerable<Assessment>> GetByModuleIdAsync(Guid moduleId);
        Task<IEnumerable<AssessmentQuestion>> GetAssessmentQuestionsAsync(Guid assessmentId);
        Task<Assessment?> GetAssessmentWithQuestionsAsync(Guid assessmentId);

        Task<AssessmentQuestion?> GetQuestionByIdAsync(Guid questionId);

        Task AddQuestionAsync(AssessmentQuestion question);
        Task<AssessmentResultDto?> GetLatestResultAsync(Guid userId, Guid assessmentId);
        Task AddQuestionsAsync(IEnumerable<AssessmentQuestion> questions);

        Task UpdateQuestionAsync(AssessmentQuestion question);
        Task DeleteQuestionAsync(AssessmentQuestion question);
        Task<bool> IsAssessmentUnlockedAsync(Guid assessmentId, Guid userId);
        Task<UserAssessmentResult?> GetUserResultAsync(Guid assessmentId, Guid userId);
        Task SaveUserResultAsync(UserAssessmentResult result);
    }
}
