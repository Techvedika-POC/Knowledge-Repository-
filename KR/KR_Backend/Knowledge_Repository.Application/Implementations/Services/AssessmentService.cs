using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System.Text.Json;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IAssessmentRepository _repo;
        private readonly IUserProgressRepository _progressRepo;

        public AssessmentService(
            IAssessmentRepository repo,
            IUserProgressRepository progressRepo)
        {
            _repo = repo;
            _progressRepo = progressRepo;
        }
        private static DateTime UnspecNow() =>
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        private static string EnsureJson(string input, string fallback = "[]")
        {
            try { JsonDocument.Parse(input); return input; }
            catch { return fallback; }
        }

        public async Task<AssessmentDto?> GetAssessmentByIdAsync(Guid id)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return null;

            return new AssessmentDto
            {
                AssessmentId = a.AssessmentId,
                ModuleId = a.ModuleId,
                TopicId = a.TopicId,
                Title = a.Title,
                Description = a.Description,
                Difficulty = a.Difficulty ?? 1,
                Metadata = EnsureJson(a.Metadata, "{}"),

                Questions = a.AssessmentQuestions.Select(q => new AssessmentQuestionDto
                {
                    QuestionId = q.QuestionId,
                    Question = q.Question,
                    Options = EnsureJson(q.Options),
                    CorrectAnswer = q.CorrectAnswer,
                    Explanation = q.Explanation,
                    Hint = q.Hint,
                    QuestionType = q.QuestionType
                }).ToList()
            };
        }

        public async Task<IEnumerable<AssessmentDto>> GetAssessmentsByModuleIdAsync(Guid moduleId)
        {
            var list = await _repo.GetByModuleIdAsync(moduleId);

            return list.Select(a => new AssessmentDto
            {
                AssessmentId = a.AssessmentId,
                ModuleId = a.ModuleId,
                Title = a.Title,
                Difficulty = a.Difficulty ?? 0
            });
        }

        public async Task<AssessmentDto> CreateAssessmentAsync(AssessmentDto dto)
        {
            var entity = new Assessment
            {
                AssessmentId = Guid.NewGuid(),
                ModuleId = dto.ModuleId,
                TopicId = dto.TopicId,
                Title = dto.Title,
                Description = dto.Description,
                Difficulty = dto.Difficulty,
                Metadata = EnsureJson(dto.Metadata, "{}"),
                LearningObjectives = dto.LearningObjectives,
                AssessmentType = dto.AssessmentType,
                EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                CreatedOn = UnspecNow()    // FIXED
            };

            await _repo.AddAsync(entity);
            dto.AssessmentId = entity.AssessmentId;

            return dto;
        }

        public async Task<IEnumerable<AssessmentDto>> CreateAssessmentsBatchAsync(IEnumerable<AssessmentDto> dtos)
        {
            var output = new List<AssessmentDto>();

            foreach (var dto in dtos)
                output.Add(await CreateAssessmentAsync(dto));

            return output;
        }

        public async Task UpdateAssessmentAsync(AssessmentDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.AssessmentId);
            if (entity == null) return;

            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.Difficulty = dto.Difficulty;
            entity.AssessmentType = dto.AssessmentType;
            entity.LearningObjectives = dto.LearningObjectives;
            entity.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
            entity.Metadata = EnsureJson(dto.Metadata, "{}");
            entity.UpdatedOn = UnspecNow();   // FIXED

            await _repo.UpdateAsync(entity);
        }

        // ---------------------------------------------------------
        // DELETE ASSESSMENT
        // ---------------------------------------------------------
        public async Task DeleteAssessmentAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity != null)
                await _repo.DeleteAsync(entity);
        }

        // ---------------------------------------------------------
        // QUESTIONS
        // ---------------------------------------------------------
        public async Task AddQuestionsAsync(Guid id, IEnumerable<AssessmentQuestionDto> questions)
        {
            var newOnes = questions.Select(q => new AssessmentQuestion
            {
                QuestionId = Guid.NewGuid(),
                AssessmentId = id,
                Question = q.Question,
                Options = EnsureJson(q.Options),
                CorrectAnswer = q.CorrectAnswer,
                Explanation = q.Explanation,
                Hint = q.Hint,
                QuestionType = q.QuestionType,
                CreatedOn = UnspecNow()    // FIXED
            });

            await _repo.AddQuestionsAsync(newOnes);
        }

        public async Task UpdateQuestionAsync(AssessmentQuestionDto dto)
        {
            var q = await _repo.GetQuestionByIdAsync(dto.QuestionId);
            if (q == null) return;

            q.Question = dto.Question;
            q.Options = EnsureJson(dto.Options);
            q.CorrectAnswer = dto.CorrectAnswer;
            q.Explanation = dto.Explanation;
            q.Hint = dto.Hint;
            q.QuestionType = dto.QuestionType;
            q.UpdatedOn = UnspecNow();   // FIXED

            await _repo.UpdateQuestionAsync(q);
        }

        public async Task DeleteQuestionAsync(Guid questionId)
        {
            var q = await _repo.GetQuestionByIdAsync(questionId);
            if (q != null)
                await _repo.DeleteQuestionAsync(q);
        }

        // ---------------------------------------------------------
        // UNLOCK LOGIC
        // ---------------------------------------------------------
        public async Task<bool> IsAssessmentUnlockedAsync(Guid assessmentId, Guid userId)
        {
            return await _repo.IsAssessmentUnlockedAsync(assessmentId, userId);
        }

        // ---------------------------------------------------------
        // SUBMIT ASSESSMENT AND EVALUATE
        // ---------------------------------------------------------
        public async Task<AssessmentResultDto> SubmitAssessmentAsync(SubmitAssessmentDto dto)
        {
            // 1️⃣ Validate USER exists
            var userExists = await _progressRepo.UserExistsAsync(dto.UserId);
            if (!userExists)
                throw new Exception($"User does not exist: {dto.UserId}");

            var assessment = await _repo.GetByIdAsync(dto.AssessmentId);
            if (assessment == null)
                throw new Exception("Assessment not found");

            var questions = assessment.AssessmentQuestions.ToList();
            var answers = JsonSerializer.Deserialize<Dictionary<string, string>>(dto.UserAnswers);

            int correct = 0;

            foreach (var q in questions)
            {
                if (answers.TryGetValue(q.QuestionId.ToString(), out var ans) &&
                    ans == q.CorrectAnswer)
                {
                    correct++;
                }
            }

            double percent = (double)correct / questions.Count * 100;
            bool passed = percent >= 60;

            var result = new UserAssessmentResult
            {
                ResultId = Guid.NewGuid(),
                UserId = dto.UserId,
                AssessmentId = dto.AssessmentId,
                UserAnswers = dto.UserAnswers,
                ScorePercentage = percent,
                Passed = passed,
                IsCompleted = true,
                AttemptedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UpdatedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            await _repo.SaveUserResultAsync(result);

            await _progressRepo.UpdateTestStatusAsync(
                dto.UserId,
                dto.WeekId,
                assessment.ModuleId!.Value,
                passed ? "Passed" : "Failed"
            );

            return new AssessmentResultDto
            {
                ResultId = result.ResultId,
                AssessmentId = dto.AssessmentId,
                UserId = dto.UserId,
                ScorePercentage = percent,
                Passed = passed,
                AttemptedOn = result.AttemptedOn,
                IsCompleted = true,
                UserAnswers = result.UserAnswers
            };
        }


        // ---------------------------------------------------------
        // GET USER ASSESSMENT RESULT
        // ---------------------------------------------------------
        public async Task<AssessmentResultDto?> GetUserAssessmentResultAsync(Guid assessmentId, Guid userId)
        {
            var r = await _repo.GetUserResultAsync(assessmentId, userId);
            if (r == null) return null;

            return new AssessmentResultDto
            {
                ResultId = r.ResultId,
                AssessmentId = r.AssessmentId,
                UserId = r.UserId,
                UserAnswers = r.UserAnswers,
                ScorePercentage = r.ScorePercentage,
                Passed = r.Passed,
                IsCompleted = r.IsCompleted,
                AttemptedOn = r.AttemptedOn
            };
        }
    }
}
