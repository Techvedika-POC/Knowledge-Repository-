using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Knowledge_Repository.Application.Implementations.Services
{
    public class VLearnTopicService : IVLearnTopicService
    {
        private readonly IVLearnTopicRepository _topicRepository;

        public VLearnTopicService(IVLearnTopicRepository topicRepository)
        {
            _topicRepository = topicRepository;
        }

        public async Task<IEnumerable<Topic>> GetAllTopicsAsync()
        {
            return await _topicRepository.GetAllTopicsAsync();
        }
    }
}
