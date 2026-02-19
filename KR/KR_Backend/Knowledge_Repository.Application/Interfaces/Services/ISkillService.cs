using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ISkillService
    {
        Task AddSkillAsync(AddSkillDto dto);
        Task UpdateUserSkillAsync(UpdateUserSkillDto dto);
        Task<List<UserSkillResponseDto>> GetUserSkillsAsync(Guid userId);
        Task<SkillSummaryDto> GetSkillSummaryAsync(Guid userId);
    }

}
