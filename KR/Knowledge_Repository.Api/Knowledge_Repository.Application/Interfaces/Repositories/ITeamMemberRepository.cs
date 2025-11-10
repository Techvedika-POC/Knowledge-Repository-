using Knowledge_Repository.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface ITeamMemberRepository : IGenericRepository<TeamMember>
    {
        Task<bool> IsUserRegisteredForEventAsync(Guid userId, Guid eventId);
    }
}
