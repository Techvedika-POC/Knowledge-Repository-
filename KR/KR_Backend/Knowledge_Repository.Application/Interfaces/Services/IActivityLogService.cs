using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IActivityLogService
    {
        
        Task<IEnumerable<ActivityLogDto>> GetUserContributionsAsync(Guid userId);

        Task<ActivityLogDto> GetContributionDetailsAsync(Guid itemId);

        Task<IEnumerable<ActivityLogDto>> GetUserContributionsFilteredAsync(
            Guid userId,
            string domain = null,
            string category = null,
            string title = null,
            string status = null,
            DateTime? date = null
        );

        Task<IEnumerable<string>> GetUserDomainsAsync(Guid userId);

        Task<IEnumerable<string>> GetUserCategoriesAsync(Guid userId);

        Task<IEnumerable<string>> GetUserTitlesAsync(Guid userId);

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

        Task<IEnumerable<ActivityLogDto>> GetUserContributionsThisMonthAsync(Guid userId);

        Task<IEnumerable<KnowledgeItemDto>> GetUserFavouritesAsync(Guid userId);
    }
}
