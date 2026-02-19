using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class LearningEventService : ILearningEventService
    {
        private readonly ILearningEventRepository _repo;
        private readonly ISkillInferenceService _skillInference;

        public LearningEventService(
            ILearningEventRepository repo,
            ISkillInferenceService skillInference)
        {
            _repo = repo;
            _skillInference = skillInference;
        }

        public async Task LogAndProcessAsync(
            Guid userId,
            string eventType,
            string entityType,
            Guid? entityId,
            string? metadata = null)
        {
            await _repo.LogAsync(
                userId,
                eventType,
                entityType,
                entityId,
                metadata
            );

            await _skillInference.InferFromActivityAsync(
                userId,
                $"{eventType}: {metadata}"
            );
        }
    }

}
