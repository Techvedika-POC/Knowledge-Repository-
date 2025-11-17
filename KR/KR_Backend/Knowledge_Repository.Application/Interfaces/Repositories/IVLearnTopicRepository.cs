using Knowledge_Repository.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IVLearnTopicRepository : IGenericRepository<Topic>
    {
        Task<IEnumerable<Topic>> GetAllTopicsAsync();
        Task<Topic> AddTopicAsync(Topic topic);
        Task<bool> TopicNameExistsAsync(string topicName);
        Task<(IEnumerable<Topic> Items, int Total)> SearchTopicsAsync(string? q, int page, int size);

    }
}
