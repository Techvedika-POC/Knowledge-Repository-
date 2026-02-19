using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class JuryAnalyticsRepository : IJuryAnalyticsRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public JuryAnalyticsRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task AddFinalScoreAsync(JuryFinalScore final)
        {
            _context.JuryFinalScores.Add(final);
            await _context.SaveChangesAsync();
        }

        public async Task AddScoreDetailAsync(JuryScoreDetail detail)
        {
            _context.JuryScoreDetails.Add(detail);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RadarChartDto>> GetRadarDataAsync(Guid teamId)
        {
            return await _context.JuryScoreDetails
                .Where(x => x.Final.TeamId == teamId)
                .Select(x => new RadarChartDto
                {
                    CriteriaName = x.CriteriaName,
                    Score = (float)x.Score
                })
                .ToListAsync();
        }

        public async Task<List<TeamRankingDto>> GetTeamRankingsAsync(Guid eventId)
        {
            return await _context.JuryFinalScores
                .Where(x => x.EventId == eventId)
                .Select(x => new TeamRankingDto
                {
                    TeamId = x.TeamId,
                    TeamName = x.Team.TeamName,
                    EventTitle = x.Event.Title,
                    MemberCount = x.Team.TeamMembers.Count(),
                    TotalScore = x.TotalScore ?? 0
                })
                .OrderByDescending(x => x.TotalScore)
                .ToListAsync();
        }

    }



}
