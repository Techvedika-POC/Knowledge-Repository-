using Knowledge_Repository.Domain.Entities;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<Team?> GetTeamByEventAndUserAsync(Guid eventId, Guid userId);
    }
}
