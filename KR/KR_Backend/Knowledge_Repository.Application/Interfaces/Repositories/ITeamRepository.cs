using Knowledge_Repository.Domain.Entities;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<Team?> GetTeamByEventAndUserAsync(Guid eventId, Guid userId);
        Task<IEnumerable<User>> GetTeamMembersAsync(Guid teamId);
        Task<List<Team>> GetTeamsByEventAsync(Guid eventId);
        Task<List<Team>> GetByEventIdAsync(Guid eventId);

    }
}
