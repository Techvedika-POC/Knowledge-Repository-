using Knowledge_Repository.Domain.Entities;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IDaySpotlightRepository : IGenericRepository<SpotlightItem>
    {
        Task<SpotlightItem?> GetLatestSpotlightAsync();
    }
}
