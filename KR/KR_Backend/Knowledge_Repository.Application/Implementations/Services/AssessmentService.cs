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
            q.UpdatedOn = UnspecNow();   // FIXED

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

            if (!await _progressRepo.UserExistsAsync(dto.UserId))
                throw new Exception($"User does not exist: {dto.UserId}");

            var assessment = await _repo.GetByIdAsync(dto.AssessmentId);
            if (assessment == null)
                throw new Exception("Assessment not found");

            var questions = assessment.AssessmentQuestions.ToList();

            var answers =
                JsonSerializer.Deserialize<Dictionary<string, string>>(dto.UserAnswers)
                ?? new Dictionary<string, string>();

            static string Normalize(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "";

                try { s = System.Net.WebUtility.HtmlDecode(s); } catch { }

                s = s.Replace("\u00A0", " ")
                     .Replace("\u200B", "")
                     .Replace("\u200C", "")
                     .Replace("\u200D", "")
                     .Replace("\uFEFF", "")
                     .Trim();

                s = s.Replace("–", "-")
                     .Replace("—", "-")
                     .Replace("‒", "-")
                     .Replace("−", "-");

                s = System.Text.RegularExpressions.Regex.Replace(
                    s,
                    @"^\s*(?:[A-Za-z]|\d+)\s*[\.\)\:\-\–]\s*",
                    ""
                );

                s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ");
                return s.Trim().ToLowerInvariant();
            }
            int correct = 0;

            foreach (var q in questions)
            {
                string qKey = q.QuestionId.ToString();
                if (!answers.TryGetValue(qKey, out string userRaw))
                    continue;

                string userNorm = Normalize(userRaw);
                string correctNorm = Normalize(q.CorrectAnswer ?? "");

                if (q.QuestionType == "MCQ")
                {
                    if (userNorm.Length == 1 && char.IsLetter(userNorm[0]))
                    {
                        var options =
                            JsonSerializer.Deserialize<string[]>(q.Options ?? "[]")
                            ?? Array.Empty<string>();

                        int index = char.ToUpperInvariant(userNorm[0]) - 'A';
                        if (index >= 0 && index < options.Length)
                        {
                            if (Normalize(options[index]) == correctNorm)
                                correct++;
                        }
                    }
                    else if (userNorm == correctNorm)
                    {
                        correct++;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(userNorm))
                        correct++;
                }
            }
            double percent = questions.Count == 0
                ? 0
                : (double)correct / questions.Count * 100;

            if (double.IsNaN(percent) || double.IsInfinity(percent))
                percent = 0;

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
                AttemptedOn = UnspecNow(),
                UpdatedOn = UnspecNow()
            };

            await _repo.SaveUserResultAsync(result); 

            if (passed)
            {
                Guid weekId = dto.WeekId;   

                if (!assessment.ModuleId.HasValue)
                    throw new Exception("Assessment.ModuleId is missing");

                Guid moduleId = assessment.ModuleId.Value;

                var lessonId = assessment.TopicId;
                if (lessonId == Guid.Empty)
                    throw new Exception("Assessment.TopicId (LessonId) missing");

                var progress = await _progressRepo.GetModuleProgressAsync(
                    dto.UserId,
                    weekId,
                    moduleId
                );

                if (progress == null)
                {
                    await _progressRepo.InitializeModuleProgressAsync(
                        dto.UserId,
                        weekId,
                        moduleId
                    );

                    progress = await _progressRepo.GetModuleProgressAsync(
                        dto.UserId,
                        weekId,
                        moduleId
                    );
                }


                progress.TestStatus = "Passed";
                progress.TopicId = lessonId;
                progress.CurrentLessonId = lessonId;
                progress.TestAttemptedOn = UnspecNow();
                progress.UpdatedAt = UnspecNow();

                await _progressRepo.UpdateModuleProgressAsync(progress);
                await _progressRepo.TryMarkModuleCompletedAsync(
                    dto.UserId,
                    weekId,
                    moduleId
                );
            }


            return new AssessmentResultDto
            {
                ResultId = result.ResultId,
                AssessmentId = dto.AssessmentId,
                UserId = dto.UserId,
                ScorePercentage = percent,
                Passed = passed,
                IsCompleted = true,
                AttemptedOn = result.AttemptedOn,
                UserAnswers = result.UserAnswers
            };
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
