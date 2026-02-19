using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IJuryAnalyticsService
    {
        Task SubmitScoreAsync(SubmitJuryScoreDto dto);
        Task<List<RadarChartDto>> GetRadarChartAsync(Guid teamId);
        Task<List<TeamRankingDto>> GetRankingsAsync(Guid eventId);

    }

}
