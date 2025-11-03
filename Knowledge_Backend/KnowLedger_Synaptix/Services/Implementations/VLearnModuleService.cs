using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class VLearnModuleService : IVLearnModuleService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public VLearnModuleService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        //  Fetch modules and apply locking logic
        public async Task<IEnumerable<VLearnModuleDto>> GetModulesByTopicAndUserAsync(Guid topicId, Guid userId)
        {
            //  Get all modules under this topic (ordered)
            var modules = await _context.Modules
                .Where(m => m.TopicId == topicId)
                .OrderBy(m => m.OrderNo)
                .ToListAsync();

            //  Get only the user's completed module progresses
            var completedProgress = await _context.UserModuleProgresses
                .Where(p => p.TopicId == topicId && p.UserId == userId &&
                            p.Status == "Completed" && p.TestStatus == "Passed")
                .Select(p => p.ModuleId)
                .ToListAsync();

            // Build module DTO list
            var moduleDtos = modules.Select(module => new VLearnModuleDto
            {
                ModuleId = module.ModuleId,
                ModuleName = module.ModuleName,
                Description = module.Description,
                ContentLink = module.ContentLink,
                OrderNo = module.OrderNo,
                Status = completedProgress.Contains(module.ModuleId) ? "Completed" : "Not Started",
                TestStatus = completedProgress.Contains(module.ModuleId) ? "Passed" : "Not Started",
                IsLocked = true 
            }).ToList();

            //  Unlock logic: unlock modules until one uncompleted module is found
            bool unlockNext = true;
            foreach (var mod in moduleDtos)
            {
                if (unlockNext)
                    mod.IsLocked = false;

                // If this module is NOT completed, stop unlocking after this one
                if (mod.Status != "Completed" || mod.TestStatus != "Passed")
                    unlockNext = false;
            }

            return moduleDtos;
        }


        //  Update module test result
        public async Task<bool> UpdateTestStatusAsync(VLearnTestResultDto result)
        {
            var progress = await _context.UserModuleProgresses
                .FirstOrDefaultAsync(p => p.UserId == result.UserId && p.ModuleId == result.ModuleId);

            if (progress == null)
            {
                // create progress record if not exists
                progress = new UserModuleProgress
                {
                    UserId = result.UserId,
                    TopicId = result.TopicId,
                    ModuleId = result.ModuleId
                };
                _context.UserModuleProgresses.Add(progress);
            }

            // Determine new status and update correctly
            if (result.TestStatus == "Passed")
            {
                progress.Status = "Completed";
                progress.TestStatus = "Passed";
                progress.CompletedOn = DateTime.UtcNow;
            }
            else if (result.TestStatus == "Failed")
            {
                progress.Status = "In Progress";
                progress.TestStatus = "Failed";
            }

            //  Common updates
            progress.TestAttemptedOn = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
