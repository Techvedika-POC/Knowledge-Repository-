using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IUserPlanEnrollmentRepository
    {
        Task<UserPlanEnrollment?> GetAsync(Guid userId, Guid planId);
        Task EnrollAsync(Guid userId, Guid planId, Guid assignedBy);
        Task StartAsync(Guid userId, Guid planId);
        Task CompleteAsync(Guid userId, Guid planId);
    }

}
