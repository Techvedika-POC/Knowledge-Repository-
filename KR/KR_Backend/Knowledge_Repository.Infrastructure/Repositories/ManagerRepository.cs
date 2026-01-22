using Knowledge_Repository.Application.Dtos;
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
    public class ManagerRepository : IManagerRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public ManagerRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        // ---------------- ASSIGN PLAN ----------------
        public async Task AssignLearningPlanAsync(Guid planId, Guid managerId, List<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                var exists = await _context.UserLearningPlans
                    .AnyAsync(x => x.PlanId == planId && x.UserId == userId);

                if (!exists)
                {
                    _context.UserLearningPlans.Add(new UserLearningPlan
                    {
                        UserLearningPlanId = Guid.NewGuid(),
                        UserId = userId,
                        PlanId = planId,
                        AssignedBy = managerId,
                        AssignedOn = DateTime.UtcNow,
                        Status = "Assigned",
                        ProgressPercent = 0
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // ---------------- PLAN PROGRESS ----------------
        public async Task<List<UserLearningProgressDto>> GetPlanProgressAsync(Guid planId)
        {
            var query =
                from ulp in _context.UserLearningPlans
                join u in _context.Users on ulp.UserId equals u.UserId
                where ulp.PlanId == planId
                select new UserLearningProgressDto
                {
                    UserId = u.UserId,
                    UserName = u.Name,
                    PlanStatus = ulp.Status,
                    ProgressPercent = ulp.ProgressPercent ?? 0,

                    TotalModules = _context.UserModuleProgresses
                        .Count(m => m.UserId == u.UserId),

                    CompletedModules = _context.UserModuleProgresses
                        .Count(m => m.UserId == u.UserId && m.Status == "Completed"),

                    LatestAssessmentScore = _context.UserAssessmentResults
                        .Where(a => a.UserId == u.UserId)
                        .OrderByDescending(a => a.AttemptedOn)
                        .Select(a => (double?)a.ScorePercentage)
                        .FirstOrDefault(),

                    Passed = _context.UserAssessmentResults
                        .Where(a => a.UserId == u.UserId)
                        .OrderByDescending(a => a.AttemptedOn)
                        .Select(a => (bool?)a.Passed)
                        .FirstOrDefault()
                };

            return await query.ToListAsync(); 
        }

        // ---------------- GET ALL PLANS ----------------
        public async Task<List<LearningPlanDto>> GetAllLearningPlansAsync()
        {
            return await _context.LearningPlans
                .Select(p => new LearningPlanDto
                {
                    PlanId = p.PlanId,
                    Title = p.Title,
                    Description = p.Description
                })
                .ToListAsync();
        }
    }
}
