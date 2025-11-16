using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IVLearnTopicService
    {
        Task<IEnumerable<Topic>> GetAllTopicsAsync();
        Task<VLearnTopicDto> GetTopicByIdAsync(Guid topicId);
        Task<VLearnTopicDto> AddTopicAsync(CreateTopicDto dto, Guid createdBy);

        Task<(IEnumerable<VLearnTopicDto> Items, int Total)> SearchTopicsAsync(string q, int page, int size);

    }
}