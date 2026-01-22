
using Knowledge_Repository.Application.Dtos.JuryCommunication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IJuryPanelService
    {
        Task<List<TeamWithMembersDto>> GetTeamsWithMembersByEventAsync(Guid eventId);
        Task<Guid> SubmitFinalScoreAsync(FinalScoreDto dto);
    }
}
