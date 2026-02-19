using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class HackathonJuryService : IHackathonJuryService
    {
        private readonly ITeamRepository _teamRepo;
        private readonly IIdeaSubmissionRepository _ideaRepo;
        private readonly IAiService _aiService;
        private readonly IAiRepository _aiRepo;

        public HackathonJuryService(
            ITeamRepository teamRepo,
            IIdeaSubmissionRepository ideaRepo,
            IAiService aiService,
            IAiRepository aiRepo)
        {
            _teamRepo = teamRepo;
            _ideaRepo = ideaRepo;
            _aiService = aiService;
            _aiRepo = aiRepo;
        }

        public async Task<List<HackathonJuryTeamDto>> GetTeamsForJuryAsync(Guid eventId)
        {
            var teams = await _teamRepo.GetByEventIdAsync(eventId);
            var ideas = await _ideaRepo.GetByEventAsync(eventId);

            var result = new List<HackathonJuryTeamDto>();

            foreach (var team in teams)
            {
                var idea = ideas.FirstOrDefault(x => x.TeamId == team.TeamId);
                var ai = await _aiRepo.GetLatestInsightAsync("Idea", team.TeamId);

                result.Add(new HackathonJuryTeamDto
                {
                    TeamId = team.TeamId,
                    TeamName = team.TeamName,

                    IdeaText = idea == null
                        ? null
                        : $"{idea.IdeaTitle}\n\n{idea.IdeaDescription}",

                    RepoUrl = idea?.RepoUrl,
                    DemoUrl = idea?.DemoUrl,

                    AiScore = (float?)ai?.Score,
                    AiFeedback = ai?.OutputResult
                });
            }

            return result;
        }

        public async Task<AiInsightResponseDto> EvaluateIdeaAsync(Guid teamId, Guid eventId)
        {
            var idea = await _ideaRepo.GetByTeamAsync(teamId);
            if (idea == null)
                throw new Exception("No idea submitted");

            var context = $"{idea.IdeaTitle}\n\n{idea.IdeaDescription}";

            return await _aiService.GenerateInsightAsync(
                new GenerateAiInsightRequestDto
                {
                    EntityType = "Idea",
                    TeamId = teamId,
                    EventId = eventId,
                    Context = context,
                    InsightType = "HackathonIdea"
                });
        }
    }


}
