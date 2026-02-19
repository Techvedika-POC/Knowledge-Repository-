using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IUserProgressService
    {
        Task EnrollUserToPlanAsync(Guid userId, Guid planId, Guid assignedBy);
        Task StartPlanAsync(Guid userId, Guid planId);
        Task StartLessonAsync(Guid userId, Guid lessonId, Guid moduleId);
        Task CompleteLessonAsync(Guid userId, Guid lessonId);
        Task StartAssessmentAsync(Guid userId, Guid assessmentId, Guid moduleId);
        Task<AssessmentResultDto> SubmitAssessmentAsync(SubmitAssessmentDto dto);

        Task<decimal> GetModuleProgressAsync(Guid userId, Guid moduleId);
        Task<decimal> GetPlanProgressAsync(Guid userId, Guid planId);
    }


}
