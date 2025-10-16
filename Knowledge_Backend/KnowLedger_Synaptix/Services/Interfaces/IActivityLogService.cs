using KnowLedger_Synaptix.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IActivityLogService
    {
        /// <summary>
        /// Retrieves all contributions of a specific user.
        /// </summary
        Task<IEnumerable<ActivityLogDto>> GetUserContributionsAsync(Guid userId);

        /// <summary>
        /// Retrieves detailed information for a specific contribution by its ID.
        /// </summary>
        Task<ActivityLogDto> GetContributionDetailsAsync(Guid itemId);

        /// <summary>
        /// Retrieves contributions of a specific user filtered by domain, category, title, status, and/or date.
        /// </summary>
        Task<IEnumerable<ActivityLogDto>> GetUserContributionsFilteredAsync(
         Guid userId,
         string domain = null,
         string category = null,
         string title = null,
         string status = null,
         DateTime? date = null);
        /// <summary>
        /// Retrieves distinct domains associated with a specific user's contributions.
        /// </summary>
        Task<IEnumerable<string>> GetUserDomainsAsync(Guid userId);
        /// <summary>
        /// Retrieves distinct categories associated with a specific user's contributions.
        /// </summary>
        Task<IEnumerable<string>> GetUserCategoriesAsync(Guid userId);
        /// <summary>
        /// Retrieves distinct titles of contributions made by a specific user.
        /// </summary>
        Task<IEnumerable<string>> GetUserTitlesAsync(Guid userId);
        /// <summary>
        /// Retrieves contributions of a user with pagination and optional filtering.
        /// </summary>
        Task<PagedResult<ActivityLogDto>> GetUserContributionsPagedAsync(
            Guid userId,
            int pageNumber = 1,
            int pageSize = 10,
            string domain = null,
            string category = null,
            string title = null,
            string status = null,
            DateTime? date = null
        );
        /// <summary>
        /// Retrieves contributions of a user with pagination and optional filtering.
        /// </summary>
        Task<IEnumerable<ActivityLogDto>> GetUserContributionsThisMonthAsync(Guid userId);

        
    }
}
