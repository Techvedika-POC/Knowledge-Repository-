using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class InterviewRepository : IInterviewRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public InterviewRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(InterviewSession session)
        {
            _context.InterviewSessions.Add(session);
            await _context.SaveChangesAsync();
        }

        public async Task<InterviewSession> GetByIdAsync(Guid interviewId)
        {
            return await _context.InterviewSessions
                .FirstOrDefaultAsync(x => x.InterviewId == interviewId);
        }

        public async Task UpdateAsync(InterviewSession session)
        {
            _context.InterviewSessions.Update(session);
            await _context.SaveChangesAsync();
        }
    }

}
