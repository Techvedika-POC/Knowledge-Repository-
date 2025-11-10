using Knowledge_Repository.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IVLearnTopicRepository : IGenericRepository<Topic>
    {
        Task<IEnumerable<Topic>> GetAllTopicsAsync();
    }
}
