using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IJuryAnalyticsRepository
    {
        Task AddFinalScoreAsync(JuryFinalScore final);
        Task AddScoreDetailAsync(JuryScoreDetail detail);
        Task<List<RadarChartDto>> GetRadarDataAsync(Guid teamId);
        Task<List<TeamRankingDto>> GetTeamRankingsAsync(Guid eventId);
    }

}
