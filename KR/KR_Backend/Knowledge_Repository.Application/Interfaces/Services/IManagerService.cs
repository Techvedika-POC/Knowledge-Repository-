using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    

    public interface IManagerService
    {
        Task AssignLearningPlanAsync(Guid planId, Guid managerId, List<Guid> userIds);

        Task<List<UserLearningProgressDto>> GetPlanProgressAsync(Guid planId);

        Task<List<LearningPlanDto>> GetAllLearningPlansAsync();  
    }


}
