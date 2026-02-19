using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class UserAssessmentProgressRepository
        : IUserAssessmentProgressRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public UserAssessmentProgressRepository(
            Knowledge_Repository_dbContext context)
        {
            _context = context;
        }
        public async Task<UserAssessmentProgress?> GetAsync(
            Guid userId, Guid assessmentId)
        {
            return await _context.UserAssessmentProgresses
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.AssessmentId == assessmentId);
        }

        public async Task StartAsync(
            Guid userId, Guid assessmentId, Guid moduleId)
        {
            var progress = await GetAsync(userId, assessmentId);

            if (progress == null)
            {
                progress = new UserAssessmentProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AssessmentId = assessmentId,
                    ModuleId = moduleId,
                    Status = "InProgress",
                    StartedOn = DateTime.UtcNow,
                    Attempts = 1
                };

                _context.UserAssessmentProgresses.Add(progress);
            }
            else
            {
                progress.Status = "InProgress";
                progress.StartedOn = DateTime.UtcNow;
                progress.Attempts++;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SubmitAsync(
            Guid userId,
            Guid assessmentId,
            string userAnswers,
            double score,
            bool passed)
        {
            var progress = await _context.UserAssessmentProgresses
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.AssessmentId == assessmentId);

            if (progress == null)
                throw new Exception("Assessment not started");

            progress.Status = passed ? "Passed" : "Failed";
            progress.Score = (decimal)score;
            progress.Passed = passed;
            progress.CompletedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<List<UserAssessmentProgress>> GetByModuleAsync(
            Guid userId, Guid moduleId)
        {
            return await _context.UserAssessmentProgresses
                .Where(x =>
                    x.UserId == userId &&
                    x.ModuleId == moduleId)
                .ToListAsync();
        }
    }
}
