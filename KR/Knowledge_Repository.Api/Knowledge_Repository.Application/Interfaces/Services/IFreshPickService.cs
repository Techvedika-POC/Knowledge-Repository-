using Knowledge_Repository.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    /// <summary>
    /// Service interface for fetching the latest ("fresh pick") knowledge items.
    /// </summary>
    public interface IFreshPickService
    {
        /// <summary>
        /// Retrieves the most recent published knowledge items.
        /// </summary>
        /// <param name="count">Number of items to retrieve (default 10).</param>
        Task<List<KnowledgeItemDto>> GetFreshPicksAsync(int count = 10);
    }
}
