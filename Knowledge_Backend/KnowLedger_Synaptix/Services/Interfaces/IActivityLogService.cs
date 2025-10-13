using KnowLedger_Synaptix.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
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
         DateTime? date = null);

        Task<IEnumerable<string>> GetUserDomainsAsync(Guid userId);

        Task<IEnumerable<string>> GetUserCategoriesAsync(Guid userId);

        Task<IEnumerable<string>> GetUserTitlesAsync(Guid userId);
    }
}
