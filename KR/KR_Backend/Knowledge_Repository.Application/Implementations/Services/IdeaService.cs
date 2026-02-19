using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class IdeaService : IIdeaService
    {
        private readonly IIdeaSubmissionRepository _repo;
        private readonly ITeamRepository _teamRepo;

        public IdeaService(
            IIdeaSubmissionRepository repo,
            ITeamRepository teamRepo)
        {
            _repo = repo;
            _teamRepo = teamRepo;
        }

        public async Task<IdeaSubmission?> GetByTeamAsync(Guid teamId)
        {
            return await _repo.GetByTeamAsync(teamId);
        }

        public async Task SubmitIdeaAsync(SubmitIdeaDto dto)
        {
            var team = await _teamRepo.GetByIdAsync(dto.TeamId);
            if (team == null)
                throw new Exception("Invalid team");

            if (team.EventId != dto.EventId)
                throw new Exception("Team does not belong to this event");

            var existing = await _repo.GetByTeamAsync(dto.TeamId);
            if (existing != null)
                throw new Exception("Idea already submitted");

            var idea = new IdeaSubmission
            {
                IdeaId = Guid.NewGuid(),
                EventId = dto.EventId,
                TeamId = dto.TeamId,
                IdeaTitle = dto.Title,
                IdeaDescription = dto.Description,
                RepoUrl = dto.RepoUrl,
                DemoUrl = dto.DemoUrl,
                CreatedOn = DateTime.UtcNow
            };

            await _repo.AddAsync(idea);
        }
    }

}
