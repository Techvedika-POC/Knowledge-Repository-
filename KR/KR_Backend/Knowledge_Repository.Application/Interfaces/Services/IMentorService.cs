using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Dtos.Mentor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IMentorService
    {

        Task<IEnumerable<TeamsByMonthDto>> GetTeamsForMentorAsync(Guid mentorId);

        Task<TeamDetailsDto> GetTeamDetailsAsync(Guid teamId);
      
    }
    }
