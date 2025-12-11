using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IAssessmentRepository : IGenericRepository<Assessment>
    {
        // ▬▬▬▬▬ ASSESSMENTS ▬▬▬▬▬
        Task<IEnumerable<Assessment>> GetByModuleIdAsync(Guid moduleId);

        // ▬▬▬▬▬ QUESTIONS ▬▬▬▬▬
        Task<IEnumerable<AssessmentQuestion>> GetAssessmentQuestionsAsync(Guid assessmentId);
        Task<AssessmentQuestion?> GetQuestionByIdAsync(Guid questionId);

        Task AddQuestionAsync(AssessmentQuestion question);
        Task AddQuestionsAsync(IEnumerable<AssessmentQuestion> questions);

        Task UpdateQuestionAsync(AssessmentQuestion question);
        Task DeleteQuestionAsync(AssessmentQuestion question);

        // ▬▬▬▬▬ ASSESSMENT ACCESS CONTROL ▬▬▬▬▬
        Task<bool> IsAssessmentUnlockedAsync(Guid assessmentId, Guid userId);

        // ▬▬▬▬▬ USER RESULT HANDLING ▬▬▬▬▬
        Task<UserAssessmentResult?> GetUserResultAsync(Guid assessmentId, Guid userId);
        Task SaveUserResultAsync(UserAssessmentResult result);
    }
}
