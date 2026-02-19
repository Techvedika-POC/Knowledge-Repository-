using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class SkillService : ISkillService
    {
        private readonly ISkillRepository _repo;

        public SkillService(
            ISkillRepository repo)
        {
            _repo = repo;
        }
        public async Task AddSkillAsync(AddSkillDto dto)
        {
            var existing = await _repo.GetSkillByNameAsync(dto.Name);
            if (existing != null)
                return;

            await _repo.AddSkillAsync(new Skill
            {
                SkillId = Guid.NewGuid(),
                Name = dto.Name
            });
        }

        public async Task UpdateUserSkillAsync(UpdateUserSkillDto dto)
        {
            var skill = await _repo.GetSkillByNameAsync(dto.SkillName);
            if (skill == null)
                throw new Exception($"Skill '{dto.SkillName}' not found");

            var userSkill = await _repo.GetUserSkillAsync(dto.UserId, skill.SkillId);

            if (userSkill == null)
            {
                userSkill = new UserSkill
                {
                    UserSkillId = Guid.NewGuid(),
                    UserId = dto.UserId,
                    SkillId = skill.SkillId,
                    Proficiency = dto.Proficiency
                };

                await _repo.AddOrUpdateUserSkillAsync(userSkill);
                return;
            }

            userSkill.Proficiency = dto.Proficiency;
            await _repo.AddOrUpdateUserSkillAsync(userSkill);
        }

        public async Task<List<UserSkillResponseDto>> GetUserSkillsAsync(Guid userId)
        {
            var skills = await _repo.GetSkillsByUserAsync(userId);

            return skills.Select(x => new UserSkillResponseDto
            {
                SkillName = x.Skill.Name,
                Proficiency = (float)(x.Proficiency ?? 0)
            }).ToList();
        }

        public async Task<SkillSummaryDto> GetSkillSummaryAsync(Guid userId)
        {
            var skills = await _repo.GetSkillsByUserAsync(userId);

            if (!skills.Any())
            {
                return new SkillSummaryDto
                {
                    LearningVelocity = "Unknown",
                    StrengthArea = "-",
                    GrowthArea = "-"
                };
            }

            var ordered = skills.OrderByDescending(s => s.Proficiency).ToList();

            return new SkillSummaryDto
            {
                LearningVelocity = ordered.Average(s => s.Proficiency) > 70
                    ? "High"
                    : "Moderate",
                StrengthArea = ordered.First().Skill.Name,
                GrowthArea = ordered.Last().Skill.Name
            };
        }
    }
}
