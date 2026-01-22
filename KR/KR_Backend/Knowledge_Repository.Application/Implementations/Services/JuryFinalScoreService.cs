using Knowledge_Repository.Application.Dtos.JuryCommunication;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Services
{
    public class JuryFinalScoreService : IJuryFinalScoreService
    {
        private readonly IEventRepository _eventRepo;
        private readonly ITeamRepository _teamRepo;
        private readonly IUserRepository _userRepo;
        private readonly IJuryFinalScoreRepository _repo;

        public JuryFinalScoreService(
            IEventRepository eventRepo,
            ITeamRepository teamRepo,
            IUserRepository userRepo,
            IJuryFinalScoreRepository repo)
        {
            _eventRepo = eventRepo ?? throw new ArgumentNullException(nameof(eventRepo));
            _teamRepo = teamRepo ?? throw new ArgumentNullException(nameof(teamRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<Guid> SubmitFinalScoreAsync(FinalScoreDto dto)
        {
            var ev = await _eventRepo.GetByIdAsync(dto.EventId);
            if (ev == null) throw new InvalidOperationException("Event not found.");

            var team = await _teamRepo.GetByIdAsync(dto.TeamId);
            if (team == null) throw new InvalidOperationException("Team not found.");

            if (dto.ApprovedBy.HasValue)
            {
                var user = await _userRepo.GetByIdAsync(dto.ApprovedBy.Value);
                if (user == null) throw new InvalidOperationException("Approver user not found.");
            }
            else
            {
                throw new InvalidOperationException("ApprovedBy must be provided."); 
            }


            var already = await _repo.ExistsForEventTeamByJuryAsync(dto.EventId, dto.TeamId, dto.ApprovedBy.Value);
            if (already)
                throw new InvalidOperationException("You have already submitted a final score for this team and event.");


            var entity = new JuryFinalScore
            {
                FinalId = Guid.NewGuid(),
                EventId = dto.EventId,
                TeamId = dto.TeamId,
                ApprovedBy = dto.ApprovedBy,
                TotalScore = dto.TotalScore,
                Remarks = dto.Remarks,
                ApprovedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            await _repo.AddAsync(entity);
            return entity.FinalId;
        }

    }
}
