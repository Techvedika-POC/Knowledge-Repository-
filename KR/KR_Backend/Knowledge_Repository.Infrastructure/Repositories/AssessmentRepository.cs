using Application.Interfaces;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class AssessmentRepository : GenericRepository<Assessment>, IAssessmentRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public AssessmentRepository(Knowledge_Repository_dbContext ctx) : base(ctx)
        {
            _context = ctx;
        }

        // Helper: Force DateTimeKind.Unspecified for PostgreSQL
        private static DateTime NoKind(DateTime dt) =>
            DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

        // ▬▬▬▬▬ ASSESSMENTS ▬▬▬▬▬
        public async Task<IEnumerable<Assessment>> GetByModuleIdAsync(Guid moduleId)
        {
            return await _dbSet
                .Include(a => a.AssessmentQuestions)
                .Include(a => a.UserAssessmentResults)
                .Where(a => a.ModuleId == moduleId)
                .ToListAsync();
        }

        // ▬▬▬▬▬ QUESTIONS ▬▬▬▬▬
        public async Task<IEnumerable<AssessmentQuestion>> GetAssessmentQuestionsAsync(Guid assessmentId)
        {
            return await _context.AssessmentQuestions
                .Where(q => q.AssessmentId == assessmentId)
                .ToListAsync();
        }

        public Task<AssessmentQuestion?> GetQuestionByIdAsync(Guid questionId)
        {
            return _context.AssessmentQuestions.FirstOrDefaultAsync(q => q.QuestionId == questionId);
        }

        public async Task AddQuestionAsync(AssessmentQuestion question)
        {
            await _context.AssessmentQuestions.AddAsync(question);
            await _context.SaveChangesAsync();
        }

        public async Task AddQuestionsAsync(IEnumerable<AssessmentQuestion> questions)
        {
            await _context.AssessmentQuestions.AddRangeAsync(questions);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuestionAsync(AssessmentQuestion question)
        {
            _context.AssessmentQuestions.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(AssessmentQuestion question)
        {
            _context.AssessmentQuestions.Remove(question);
            await _context.SaveChangesAsync();
        }

        // ▬▬▬▬▬ ACCESS CONTROL ▬▬▬▬▬
        public async Task<bool> IsAssessmentUnlockedAsync(Guid assessmentId, Guid userId)
        {
            var assessment = await _dbSet.FindAsync(assessmentId);
            if (assessment == null || assessment.ModuleId == null)
                return false;

            return await _context.UserModuleProgresses
                .AnyAsync(p => p.UserId == userId && p.ModuleId == assessment.ModuleId);
        }

        // ▬▬▬▬▬ USER ASSESSMENT RESULTS ▬▬▬▬▬
        public async Task<UserAssessmentResult?> GetUserResultAsync(Guid assessmentId, Guid userId)
        {
            return await _context.UserAssessmentResults
                .FirstOrDefaultAsync(r => r.AssessmentId == assessmentId && r.UserId == userId);
        }

        public async Task SaveUserResultAsync(UserAssessmentResult result)
        {
            var existing = await GetUserResultAsync(result.AssessmentId, result.UserId);

            if (existing == null)
            {
                // ensure all timestamps are unspecified
                result.AttemptedOn = DateTime.SpecifyKind(result.AttemptedOn, DateTimeKind.Unspecified);
                result.UpdatedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                await _context.UserAssessmentResults.AddAsync(result);
            }
            else
            {
                existing.UserAnswers = result.UserAnswers;
                existing.ScorePercentage = result.ScorePercentage;
                existing.Passed = result.Passed;
                existing.IsCompleted = result.IsCompleted;

                // FIXED: remove UTC
                existing.UpdatedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            await _context.SaveChangesAsync();
        }

    }
}
