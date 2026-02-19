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
        public async Task<List<UserLearningProgressDto>> GetPlanProgressAsync(Guid planId)
        {
            var assignments =
                from ulp in _context.UserLearningPlans
                join u in _context.Users on ulp.UserId equals u.UserId
                where ulp.PlanId == planId
                select new { ulp, u };

            var data = await assignments.ToListAsync();
            var result = new List<UserLearningProgressDto>();

            foreach (var row in data)
            {
                var userModules = await _context.UserModuleProgresses
                    .Where(m => m.UserId == row.u.UserId)
                    .ToListAsync();

                int totalModules = userModules.Count;
                int completedModules = userModules.Count(m => m.Status == "Completed");
                decimal progressPercent =
                    totalModules == 0
                        ? 0m
                        : Math.Round(
                            ((decimal)completedModules / totalModules) * 100m,
                            2,
                            MidpointRounding.AwayFromZero
                          );

                var assessment = await _context.UserAssessmentProgresses
                    .Where(a => a.UserId == row.u.UserId)
                    .OrderByDescending(a => a.CompletedOn)
                    .FirstOrDefaultAsync();

                string status =
                    progressPercent >= 100m ? "Completed" :
                    progressPercent > 0m ? "InProgress" :
                    "Assigned";

                result.Add(new UserLearningProgressDto
                {
                    UserId = row.u.UserId,
                    UserName = row.u.Name,

                    PlanStatus = status,

                    TotalModules = totalModules,
                    CompletedModules = completedModules,

                    ProgressPercent = progressPercent,  

                    LatestAssessmentScore = assessment?.Score != null
                        ? (double?)assessment.Score
                        : null,

                    Passed = assessment?.Passed
                });
            }

            return result;
        }
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
