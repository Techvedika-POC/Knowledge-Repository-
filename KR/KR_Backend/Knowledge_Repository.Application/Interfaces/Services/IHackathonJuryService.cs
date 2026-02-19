using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Dtos;
namespace Knowledge_Repository.Application.Interfaces.Services
{ 

        public interface IHackathonJuryService
        {
            Task<List<HackathonJuryTeamDto>> GetTeamsForJuryAsync(Guid eventId);
            Task<AiInsightResponseDto> EvaluateIdeaAsync(Guid teamId, Guid eventId);
        }
    

}
