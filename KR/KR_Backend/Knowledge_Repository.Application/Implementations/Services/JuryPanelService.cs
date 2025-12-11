// File: Knowledge_Repository.Application.Services/JuryPanelService.cs
using Knowledge_Repository.Application.Dtos.JuryCommunication;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Services
{
    public class JuryPanelService : IJuryPanelService
    {
        private readonly ITeamRepository _teamRepo;
        private readonly ITeamMemberRepository _teamMemberRepo;
        private readonly IJuryFinalScoreService _finalScoreService;

        public JuryPanelService(
            ITeamRepository teamRepo,
            ITeamMemberRepository teamMemberRepo,
            IJuryFinalScoreService finalScoreService)
        {
            _teamRepo = teamRepo ?? throw new ArgumentNullException(nameof(teamRepo));
            _teamMemberRepo = teamMemberRepo ?? throw new ArgumentNullException(nameof(teamMemberRepo));
            _finalScoreService = finalScoreService ?? throw new ArgumentNullException(nameof(finalScoreService));
        }

        public async Task<List<TeamWithMembersDto>> GetTeamsWithMembersByEventAsync(Guid eventId)
        {
            var teams = await _teamRepo.GetByEventIdAsync(eventId) ?? new List<Team>();
            var result = new List<TeamWithMembersDto>(teams.Count);

            foreach (var t in teams)
            {
                if (t == null) continue;
                var members = await _teamMemberRepo.GetMembersByTeamIdAsync(t.TeamId);

                var memberDtos = (members ?? new List<TeamMember>()).Select(m =>
                {
                    var user = m?.User;
                    return new TeamMemberDto
                    {
                        UserId = m?.UserId ?? Guid.Empty,
                        Name = user?.Name ?? string.Empty,
                        Email = user?.Email ?? string.Empty
                    };
                }).ToList();

                result.Add(new TeamWithMembersDto
                {
                    TeamId = t.TeamId,
                    TeamName = t.TeamName ?? string.Empty,
                    TeamDescription = string.Empty, 
                    Members = memberDtos
                });
            }

            return result;
        }

        public async Task<Guid> SubmitFinalScoreAsync(FinalScoreDto dto)
        { 
            return await _finalScoreService.SubmitFinalScoreAsync(dto);
        }
    }
}
