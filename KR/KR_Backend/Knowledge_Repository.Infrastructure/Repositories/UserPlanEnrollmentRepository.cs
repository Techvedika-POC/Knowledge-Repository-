using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class UserPlanEnrollmentRepository
        : IUserPlanEnrollmentRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public UserPlanEnrollmentRepository(
            Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<UserPlanEnrollment?> GetAsync(
            Guid userId, Guid planId)
        {
            return await _context.UserPlanEnrollments
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.PlanId == planId);
        }

        public async Task EnrollAsync(
            Guid userId, Guid planId, Guid assignedBy)
        {
            var existing = await GetAsync(userId, planId);
            if (existing != null) return;

            var enrollment = new UserPlanEnrollment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PlanId = planId,
                AssignedBy = assignedBy,
                AssignmentType = "Manager",
                Status = "Assigned",
                AssignedOn = DateTime.UtcNow
            };

            _context.UserPlanEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task StartAsync(Guid userId, Guid planId)
        {
            var enrollment = await GetAsync(userId, planId);
            if (enrollment == null)
                throw new InvalidOperationException(
                    "User is not enrolled in this plan.");

            enrollment.Status = "InProgress";
            enrollment.StartedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task CompleteAsync(Guid userId, Guid planId)
        {
            var enrollment = await GetAsync(userId, planId);
            if (enrollment == null) return;

            enrollment.Status = "Completed";
            enrollment.CompletedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
