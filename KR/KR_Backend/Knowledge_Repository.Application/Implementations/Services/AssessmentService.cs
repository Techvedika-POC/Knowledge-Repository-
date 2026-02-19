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
        private readonly IUserModuleProgressRepository _moduleProgressRepo;
        private readonly IUserAssessmentProgressRepository _assessmentProgressRepo;
        private readonly ILearningEventRepository _eventRepo;


        public AssessmentService(
       IAssessmentRepository repo,
       IUserModuleProgressRepository moduleProgressRepo,
       IUserAssessmentProgressRepository assessmentProgressRepo,
       ILearningEventRepository eventRepo)
        {
            _repo = repo;
            _moduleProgressRepo = moduleProgressRepo;
            _assessmentProgressRepo = assessmentProgressRepo;
            _eventRepo = eventRepo;
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
            entity.UpdatedOn = UnspecNow();  

            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAssessmentAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity != null)
                await _repo.DeleteAsync(entity);
        }

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
                CreatedOn = UnspecNow()   
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
            q.UpdatedOn = UnspecNow(); 

            await _repo.UpdateQuestionAsync(q);
        }

        public async Task DeleteQuestionAsync(Guid questionId)
        {
            var q = await _repo.GetQuestionByIdAsync(questionId);
            if (q != null)
                await _repo.DeleteQuestionAsync(q);
        }

        public async Task<bool> IsAssessmentUnlockedAsync(Guid assessmentId, Guid userId)
        {
            return await _repo.IsAssessmentUnlockedAsync(assessmentId, userId);
        }
        public async Task<AssessmentResultDto> SubmitAssessmentAsync(SubmitAssessmentDto dto)
        {
            var assessment = await _repo.GetAssessmentWithQuestionsAsync(dto.AssessmentId);
            if (assessment == null)
                throw new Exception("Assessment not found");

            var answers =
                JsonSerializer.Deserialize<Dictionary<Guid, string>>(dto.UserAnswers)
                ?? new();

            int correct = 0;
            int total = assessment.AssessmentQuestions.Count;

            foreach (var q in assessment.AssessmentQuestions)
            {
                if (answers.TryGetValue(q.QuestionId, out var selected))
                {
                    var options = JsonSerializer.Deserialize<List<string>>(q.Options ?? "[]");

                    int selectedIndex = selected.ToUpper() switch
                    {
                        "A" => 0,
                        "B" => 1,
                        "C" => 2,
                        "D" => 3,
                        _ => -1
                    };

                    if (selectedIndex >= 0 && selectedIndex < options.Count)
                    {
                        var selectedText = options[selectedIndex];

                        if (string.Equals(
                            selectedText.Trim(),
                            q.CorrectAnswer.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                        {
                            correct++;
                        }
                    }

                }
            }

            var percentage = total == 0 ? 0 : (double)correct / total * 100;
            var passed = percentage >= 60;
            await _repo.SaveUserResultAsync(new UserAssessmentResult
            {
                ResultId = Guid.NewGuid(),
                UserId = dto.UserId,
                AssessmentId = dto.AssessmentId,
                UserAnswers = dto.UserAnswers,         
                ScorePercentage = percentage,
                Passed = passed,
                IsCompleted = true,
                AttemptedOn = DateTime.UtcNow
            });

            await _assessmentProgressRepo.SubmitAsync(
                dto.UserId,
                dto.AssessmentId,
                dto.UserAnswers,
                percentage,
                passed
            );

            return new AssessmentResultDto
            {
                AssessmentId = dto.AssessmentId,
                UserId = dto.UserId,
                UserAnswers = dto.UserAnswers,
                ScorePercentage = percentage,
                Passed = passed,
                IsCompleted = true,
                AttemptedOn = DateTime.UtcNow
            };
        }



        public async Task StartAssessmentAsync(Guid assessmentId, Guid userId)
        {
            var assessment = await _repo.GetByIdAsync(assessmentId);
            if (assessment == null || assessment.ModuleId == null)
                throw new Exception("Assessment has no module");

            await _assessmentProgressRepo.StartAsync(
                userId,
                assessmentId,
                assessment.ModuleId.Value
            );
        }
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
