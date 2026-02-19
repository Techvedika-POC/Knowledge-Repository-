using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<TeamDto?> GetMyTeamForEvent(Guid eventId, Guid userId);
        Task<List<TeamDto>> GetTeamsForEvent(Guid eventId);

        Task AddMemberAsync(Guid teamId, Guid creatorId, string email);
        Task RemoveMemberAsync(Guid teamId, Guid creatorId, Guid memberId);
    }

}
