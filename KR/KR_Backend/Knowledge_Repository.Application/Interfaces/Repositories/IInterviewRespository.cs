using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IInterviewRepository
    {
        Task CreateAsync(InterviewSession session);
        Task<InterviewSession> GetByIdAsync(Guid interviewId);
        Task UpdateAsync(InterviewSession session);
    }

}
