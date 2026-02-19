using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class JuryAnalyticsService : IJuryAnalyticsService
    {
        private readonly IJuryAnalyticsRepository _repo;
        private readonly IAiService _aiService;

        public JuryAnalyticsService(
            IJuryAnalyticsRepository repo,
            IAiService aiService)
        {
            _repo = repo;
            _aiService = aiService;
        }

        public async Task SubmitScoreAsync(SubmitJuryScoreDto dto)
        {
            var final = new JuryFinalScore
            {
                FinalId = Guid.NewGuid(),
                TeamId = dto.TeamId,
                EventId = dto.EventId,
                ApprovedBy = dto.JuryId,            
                ApprovedOn = DateTime.UtcNow,          
                TotalScore = dto.Scores.Sum(s => s.Score),
                Remarks = "Jury evaluation submitted"
            };

            await _repo.AddFinalScoreAsync(final);
            foreach (var s in dto.Scores)
            {
                var detail = new JuryScoreDetail
                {
                    ScoreDetailId = Guid.NewGuid(),
                    FinalId = final.FinalId,
                    CriteriaName = s.CriteriaName,
                    Score = s.Score,
                    Remarks = s.Remarks
                };

                await _repo.AddScoreDetailAsync(detail);
            }
        }

        public async Task<List<RadarChartDto>> GetRadarChartAsync(Guid teamId)
        {
            return await _repo.GetRadarDataAsync(teamId);
        }
        public async Task<List<TeamRankingDto>> GetRankingsAsync(Guid eventId)
        {
            var rankings = await _repo.GetTeamRankingsAsync(eventId);

            var context = string.Join("\n",
                rankings.Select(r => $"Team {r.TeamId} Score: {r.TotalScore}"));

            await _aiService.GenerateInsightAsync(
                new GenerateAiInsightRequestDto
                {
                    EntityType = "JuryRanking",
                    InsightType = "RankingAnalysis",
                    EntityId = eventId,
                    Context = context
                });

            return rankings;
        }
    }



}
