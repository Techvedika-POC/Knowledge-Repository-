using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IUserAssessmentProgressRepository
    {
        Task<UserAssessmentProgress?> GetAsync(Guid userId, Guid assessmentId);
        Task StartAsync(Guid userId, Guid assessmentId, Guid moduleId);
        Task SubmitAsync(Guid userId, Guid assessmentId, string userAnswers, double score, bool passed)
;

        Task<List<UserAssessmentProgress>> GetByModuleAsync(Guid userId, Guid moduleId);
    }

}
