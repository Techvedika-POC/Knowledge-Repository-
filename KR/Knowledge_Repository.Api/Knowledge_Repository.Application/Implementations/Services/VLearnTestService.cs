using KnowLedger_Synaptix.Dtos;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class VLearnTestService : IVLearnTestService
    {
        private readonly IVLearnTestRepository _testRepository;
        private readonly ILogger<VLearnTestService> _logger;

        public VLearnTestService(
            IVLearnTestRepository testRepository,
            ILogger<VLearnTestService> logger)
        {
            _testRepository = testRepository ?? throw new ArgumentNullException(nameof(testRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates questions for a module or topic using the repository.
        /// </summary>
        public async Task<VLearnQuestionResponseDto> GenerateQuestionsAsync(VLearnQuestionRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                var result = await _testRepository.GenerateQuestionsAsync(request);

                foreach (var q in result.Questions)
                {
                    q.Options = q.Options.OrderBy(o => Guid.NewGuid()).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating questions for module {Module}", request.ModuleName);
                return new VLearnQuestionResponseDto { Questions = new System.Collections.Generic.List<VLearnQuestionDto>() };
            }
        }
    }
}
