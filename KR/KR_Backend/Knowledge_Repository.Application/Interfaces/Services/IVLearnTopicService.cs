using System.Collections.Generic;
using System.Threading.Tasks;
using Knowledge_Repository.Domain.Entities;


namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IVLearnTopicService
    {
        Task<IEnumerable<Topic>> GetAllTopicsAsync();
    }
}