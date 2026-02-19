using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface ISkillRepository
    {
        Task<Skill> GetSkillByNameAsync(string name);
        Task AddSkillAsync(Skill skill);

        Task<UserSkill> GetUserSkillAsync(Guid userId, Guid skillId);
        Task AddOrUpdateUserSkillAsync(UserSkill userSkill);

        Task<List<UserSkill>> GetSkillsByUserAsync(Guid userId);
    }

}
