using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IActivityLogRepository : IGenericRepository<ActivityLog>
    {
        Task<IEnumerable<ActivityLog>> GetUserContributionsAsync(Guid userId);
        Task<IEnumerable<ActivityLog>> GetUserContributionsFilteredAsync(
            Guid userId,
            string? domain,
            string? category,
            string? title,
            string? status,
            DateTime? date);
        Task<IEnumerable<string>> GetUserDomainsAsync(Guid userId);
        Task<IEnumerable<string>> GetUserCategoriesAsync(Guid userId);
        Task<IEnumerable<string>> GetUserTitlesAsync(Guid userId);
        Task<IEnumerable<ActivityLog>> GetUserContributionsThisMonthAsync(Guid userId);
    }
}
