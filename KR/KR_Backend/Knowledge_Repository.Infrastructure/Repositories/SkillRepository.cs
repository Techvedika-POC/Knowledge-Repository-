using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class SkillRepository : ISkillRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public SkillRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<Skill> GetSkillByNameAsync(string name)
        {
            return await _context.Skills
                .FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task AddSkillAsync(Skill skill)
        {
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();
        }

        public async Task<UserSkill> GetUserSkillAsync(Guid userId, Guid skillId)
        {
            return await _context.UserSkills
                .FirstOrDefaultAsync(x => x.UserId == userId && x.SkillId == skillId);
        }

        public async Task AddOrUpdateUserSkillAsync(UserSkill userSkill)
        {
            var exists = await _context.UserSkills
                .AnyAsync(x => x.UserSkillId == userSkill.UserSkillId);

            if (!exists)
                _context.UserSkills.Add(userSkill);
            else
                _context.UserSkills.Update(userSkill);

            await _context.SaveChangesAsync();
        }



        public async Task<List<UserSkill>> GetSkillsByUserAsync(Guid userId)
        {
            return await _context.UserSkills
                .Include(x => x.Skill)
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

    }

}
